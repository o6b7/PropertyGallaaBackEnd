using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PropertyGalla.Data;

namespace PropertyGalla.Services
{
    public class IdGeneratorService
    {
        private readonly PropertyGallaContext _context;

        public IdGeneratorService(PropertyGallaContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateIdAsync(string tableName)
        {
            string prefix = GetPrefix(tableName);
            if (prefix == null)
                throw new ArgumentException("Invalid table name or prefix not defined.");

            int nextNumber = 1;

            switch (tableName.ToLower())
            {
                case "users":
                    nextNumber = await GetNextNumber(_context.Users.OrderByDescending(u => u.UserId), prefix);
                    break;
                case "properties":
                    nextNumber = await GetNextNumber(_context.Properties.OrderByDescending(p => p.PropertyId), prefix);
                    break;
                case "viewrequests":
                    nextNumber = await GetNextNumber(_context.ViewRequests.OrderByDescending(r => r.ViewRequestId), prefix);
                    break;
                case "feedback":
                    nextNumber = await GetNextNumber(_context.Feedbacks.OrderByDescending(f => f.FeedbackId), prefix);
                    break;
                case "reports":
                    nextNumber = await GetNextNumber(_context.Reports.OrderByDescending(r => r.ReportId), prefix);
                    break;
                default:
                    throw new ArgumentException("Unknown table");
            }

            // ✅ Your requested format: PREFIX + "000" + number
            string generatedId = $"{prefix}000{nextNumber}";

            // Ensure uniqueness
            while (await IdExistsAsync(tableName, generatedId))
            {
                nextNumber++;
                generatedId = $"{prefix}000{nextNumber}";
            }

            return generatedId;
        }

        private async Task<int> GetNextNumber<T>(IQueryable<T> orderedQuery, string prefix)
        {
            var idProp = typeof(T).GetProperties().FirstOrDefault(p => p.Name.EndsWith("Id"));
            if (idProp == null) return 1;

            var lastEntity = await orderedQuery.FirstOrDefaultAsync();
            if (lastEntity != null)
            {
                var idValue = idProp.GetValue(lastEntity)?.ToString();
                return ExtractNumber(idValue, prefix) + 1;
            }
            return 1;
        }

        private async Task<bool> IdExistsAsync(string tableName, string id)
        {
            return tableName.ToLower() switch
            {
                "users" => await _context.Users.AnyAsync(u => u.UserId == id),
                "properties" => await _context.Properties.AnyAsync(p => p.PropertyId == id),
                "viewrequests" => await _context.ViewRequests.AnyAsync(r => r.ViewRequestId == id),
                "feedback" => await _context.Feedbacks.AnyAsync(f => f.FeedbackId == id),
                "reports" => await _context.Reports.AnyAsync(r => r.ReportId == id),
                _ => throw new ArgumentException("Unknown table")
            };
        }

        private string GetPrefix(string tableName)
        {
            return tableName.ToLower() switch
            {
                "users" => "USE",
                "properties" => "PRO",
                "usercart" => "CRT",
                "viewrequests" => "VRQ",
                "feedback" => "FED",
                "reports" => "REP",
                _ => null
            };
        }

        private int ExtractNumber(string id, string prefix)
        {
            if (!string.IsNullOrEmpty(id) && id.StartsWith(prefix + "000"))
            {
                string numberPart = id.Substring((prefix + "000").Length);
                return int.TryParse(numberPart, out int num) ? num : 0;
            }
            return 0;
        }
    }
}
