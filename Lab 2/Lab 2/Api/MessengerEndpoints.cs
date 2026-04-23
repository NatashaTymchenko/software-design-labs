using Microsoft.EntityFrameworkCore;
using MessengerApp.Data;
using MessengerApp.Models;

namespace MessengerApp.Api;

public static class MessengerEndpoints
{
    public static void MapMessengerEndpoints(this WebApplication app)
    {
        app.MapPost("/users", async (User user, AppDbContext db) =>
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return Results.Created($"/users/{user.Id}", user);
        });

        app.MapPost("/messages", async (Message msg, AppDbContext db) =>
        {
            msg.CreatedAt = DateTime.UtcNow;
            db.Messages.Add(msg);
            await db.SaveChangesAsync();
            return Results.Created($"/messages/{msg.Id}", msg);
        });

        app.MapGet("/conversations/{id}/messages", async (int id, AppDbContext db) =>
        {
            return await db.Messages
                .Where(m => m.ConversationId == id && !m.IsDeleted)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        });

        app.MapPut("/messages/{id}", async (int id, MessageUpdate update, AppDbContext db) =>
        {
            var msg = await db.Messages.FindAsync(id);
            if (msg == null || msg.IsDeleted) return Results.NotFound();

            db.AuditLogs.Add(new MessageAudit
            {
                MessageId = id,
                OldText = msg.Text,
                EditedAt = DateTime.UtcNow
            });

            msg.Text = update.Text;
            msg.IsEdited = true;
            await db.SaveChangesAsync();
            return Results.Ok(msg);
        });

        app.MapDelete("/messages/{id}", async (int id, AppDbContext db) =>
        {
            var msg = await db.Messages.FindAsync(id);
            if (msg == null) return Results.NotFound();

            msg.IsDeleted = true;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}