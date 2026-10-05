using ChessApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.DbContext
{
    public static class DbInitializer
    {
        /// <summary>
        /// ฟังก์ชันสร้าง Admin อัตโนมัติในฐานข้อมูลเมื่อระบบเริ่มทำงาน (ถ้ายังไม่มี)
        /// โดยกำหนดให้เป็น User ลำดับที่ 1 (user_id = 1)
        /// </summary>
        public static async Task SeedAdminAsync(ChessDbContext context, IPasswordHasher<user> passwordHasher)
        {
            try
            {
                // ตรวจสอบว่าสามารถเชื่อมต่อฐานข้อมูลได้หรือไม่
                if (!await context.Database.CanConnectAsync())
                {
                    Console.WriteLine("[DbInitializer] Database connection not available. Skipping admin seeding.");
                    return;
                }

                // ตรวจสอบว่ามีผู้ใช้อยู่ในระบบหรือไม่
                var hasUsers = await context.users.AnyAsync();
                var adminUser = await context.users.FirstOrDefaultAsync(u =>
                    u.user_id == 1 || u.username == "admin" || u.email == "admin@chess.com");

                if (adminUser == null)
                {
                    // ถ้ายังไม่มีตารางหรือเป็นตารางว่าง ผู้ใช้คนแรกจะได้รับ user_id = 1 อัตโนมัติ (AUTO_INCREMENT)
                    var admin = new user
                    {
                        user_id = 1,
                        username = "admin",
                        email = "admin@chess.com",
                        rating = 2000,
                        games_played = 0,
                        games_won = 0,
                        games_lost = 0,
                        games_drawn = 0,
                        status = "offline",
                        created_at = DateTime.UtcNow
                    };

                    // รหัสผ่านเริ่มต้นสำหรับ Admin: Admin@1234
                    admin.password_hash = passwordHasher.HashPassword(admin, "Admin@1234");

                    context.users.Add(admin);
                    await context.SaveChangesAsync();

                    Console.WriteLine($"[DbInitializer] Successfully created Admin user! (UserId: {admin.user_id}, Username: {admin.username}, Email: {admin.email})");
                }
                else
                {
                    Console.WriteLine($"[DbInitializer] Admin user already exists (UserId: {adminUser.user_id}, Username: {adminUser.username}).");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DbInitializer] Error while seeding admin user: {ex.Message}");
            }
        }
    }
}
