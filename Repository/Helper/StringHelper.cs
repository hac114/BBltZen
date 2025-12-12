using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Helper
{
    public static class StringHelper
    {
        // ✅ VERIFICA SE INIZIA CON (case-insensitive)
        public static bool StartsWithCaseInsensitive(string source, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(searchTerm))
                return false;

            return source.StartsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase);
        }

        // ✅ VERIFICA SE CONTIENE (case-insensitive)
        public static bool ContainsCaseInsensitive(string source, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(searchTerm))
                return false;

            return source.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase);
        }

        // ✅ NORMALIZZAZIONE PER RICERCA
        public static string NormalizeSearchTerm(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return input.Trim();
        }

        // ✅ RICERCA INIZIALI/MULTIPLE PAROLE
        public static bool StartsWithAnyWord(string source, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(searchTerm))
                return false;

            var searchWords = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var sourceWords = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return searchWords.Any(searchWord =>
                sourceWords.Any(sourceWord =>
                    sourceWord.StartsWith(searchWord, StringComparison.InvariantCultureIgnoreCase)));
        }

        // ✅ VERIFICA UGUAGLIANZA CASE-INSENSITIVE
        public static bool EqualsCaseInsensitive(string source, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(source) && string.IsNullOrWhiteSpace(searchTerm))
                return true;

            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(searchTerm))
                return false;

            return source.Equals(searchTerm, StringComparison.InvariantCultureIgnoreCase);
        }

        // ✅ VERIFICA UGUAGLIANZA SENZA SPAZI EXTRA
        public static bool EqualsTrimmedCaseInsensitive(string source, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(source) && string.IsNullOrWhiteSpace(searchTerm))
                return true;

            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(searchTerm))
                return false;

            return source.Trim().Equals(searchTerm.Trim(), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}