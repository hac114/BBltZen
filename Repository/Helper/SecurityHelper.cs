using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Helper
{
    // ✅ HELPER PER SICUREZZA E PAGINAZIONE
    public static class SecurityHelper
    {
        // ✅ VALIDAZIONE INPUT STRINGHE
        public static bool IsValidInput(string? input, int maxLength = 50)
        {
            // ✅ ACCETTA NULL - considera input vuoto come valido
            if (string.IsNullOrWhiteSpace(input))
                return true;

            // ✅ Pattern SQL injection e HTML/JavaScript
            var dangerousPatterns = new[]
            { 
                // SQL Injection
                ";", "--", "/*", "*/", "@@", "xp_", "sp_", "exec",
                // HTML/JavaScript
                "<script", "</script>", "javascript:",
                "onclick=", "onload=", "onerror=", "onmouseover=",
                "<iframe", "</iframe>", "<object", "</object>",
                "<embed", "</embed>", "<applet", "</applet>"
            };

            return input.Length <= maxLength &&
                   !dangerousPatterns.Any(pattern =>
                       input.Contains(pattern, StringComparison.InvariantCultureIgnoreCase));
        }

        // ✅ NORMALIZZAZIONE SICURA
        public static string NormalizeSafe(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            return input.Trim().ToUpperInvariant();
        }

        // ✅ PAGINAZIONE SICURA
        public static (int page, int pageSize) ValidatePagination(int? page, int? pageSize)
        {
            var safePage = Math.Max(1, page ?? 1);
            var safePageSize = Math.Clamp(pageSize ?? 10, 1, 100);
            return (safePage, safePageSize);
        }
    }
}
