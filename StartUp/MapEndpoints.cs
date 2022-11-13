﻿using Orders.Models;
using Microsoft.EntityFrameworkCore;
using Orders.Data;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace Orders.StartUp;

public static class MapEndpoints
{
    public static WebApplication MapProductEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => "Hello World!");

        app.MapGet("/products", async (OrdersDB db) => await db.Products.ToListAsync());

        app.MapPost("/products", async (OrdersDB db, Product product) =>
        {
            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();
            return Results.Created($"/products/{product.Id}", product);
        });

        app.MapGet("/products/{id}", async (OrdersDB db, int id) => await db.Products.FindAsync(id));

        app.MapPut("/products/{id}", async (OrdersDB db, Product update, int id) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return Results.NotFound();
            product.Name = update.Name;
            product.PriceDrebno = update.PriceDrebno;
            product.Code = update.Code;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        app.MapDelete("/products/{id}", async (OrdersDB db, int id) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null)
            {
                return Results.NoContent();
            }

            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        app.MapGet("/syncDatabase", async (OrdersDB db) =>
        {
            await ProductsLoader.SyncProductsData(db);
            return Results.Ok();
        });
        return app;
    }

    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        app.MapGet("/api/orders", async (OrdersDB db) =>
        {
            var orders = await db.Orders.ToListAsync();
            return orders;
        }).RequireAuthorization();
        app.MapGet("/api/orders/forDate({date})", async (OrdersDB db, string date) =>
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            var format = "dd-MM-yyyy";
            var dateParsed = DateTime.ParseExact(date, format, CultureInfo.InvariantCulture);

            var orders = await db.Orders.Where(order => order.PickupDate.Day == dateParsed.Day && order.PickupDate.Month == dateParsed.Month && order.PickupDate.Year == dateParsed.Year).ToListAsync();

            return orders;
        });

        app.MapGet("/api/orders/between({date1})and({date2})", async (OrdersDB db, string date1, string date2) =>
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            var format = "dd-MM-yyyy";
            var date1Parsed = DateTime.ParseExact(date1, format, CultureInfo.InvariantCulture);
            var date2Parsed = DateTime.ParseExact(date2, format, CultureInfo.InvariantCulture);

            var orders = await db.Orders.Where(order =>order.PickupDate<=date2Parsed && order.PickupDate>=date1Parsed).ToListAsync();

            return orders;
        });
        app.MapGet("/api/orders/{id}", async (OrdersDB db, int id) =>
        {
            return await db.Orders.FindAsync(id);
        });

        app.MapPost("/api/orders", async (OrdersDB db, Order order) =>
        {
            await db.Orders.AddAsync(order);
            return Results.Created($"/api/orders/{order.Id}", order);
        });

        app.MapPut("/api/orders/{id}", async (OrdersDB db, Order update, int id) =>
        {
            var order = await db.Orders.FindAsync(id);
            if(order is null)
            {
                return Results.NotFound();
            }

            order.OperatorId = update.Id;
            order.CreatedDate = update.CreatedDate;
            order.PickupDate = update.PickupDate;
            order.ClientName = update.ClientName;
            order.ClientPhone = update.ClientPhone;
            order.AdvancePaiment = update.AdvancePaiment;
            order.IsPaid = update.IsPaid;
            order.Status = update.Status;
            order.OrderItems = update.OrderItems;

            await db.SaveChangesAsync();
            return Results.Ok();

        });

        return app;
    }
    public static WebApplication MapSecurityEndpoints(this WebApplication app, ConfigurationManager config)
    {
        app.MapPost("api/security/getToken", [AllowAnonymous] (User user) =>
        {
            if (user.UserName == "kodin" && user.Password == "kodin")
            {
                var issuer = config["Jwt:Issuer"];
                var audience = config["Jwt:Audience"];
                var key = Encoding.ASCII.GetBytes(config["Jwt:Key"]);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("Id", Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Email, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(10),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jstToken = tokenHandler.WriteToken(token);
                var stringToken = tokenHandler.WriteToken(token);
                return Results.Ok(stringToken);
            }
            return Results.Unauthorized();
        });
        return app;
    }




}
