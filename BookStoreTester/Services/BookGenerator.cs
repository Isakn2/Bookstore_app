using Bogus;
using System.Linq;
using System.Collections.Generic;
using BookStoreTester.Models;

namespace BookStoreTester.Services
{
    public class BookGenerator
    {
        public List<Book> GenerateBooks(BookRequest request)
        {
            var combinedSeed = request.Seed + request.Page;
            Randomizer.Seed = new Random(combinedSeed);

            var validLocale = request.Locale switch
            {
                "de" => "de",
                "fr" => "fr",
                "en" => "en",
                _ => "en"
            };


            var faker = new Faker(validLocale);

            var bookFaker = new Faker<Book>(locale: validLocale)  // Ensure locale is used here
                .UseSeed(combinedSeed)
                .RuleFor(b => b.Index, (f, b) => (request.Page - 1) * request.PageSize + f.IndexGlobal + 1)
                .RuleFor(b => b.ISBN, f => GenerateISBN(f, validLocale))
                .RuleFor(b => b.Title, f => GenerateTitle(f, validLocale))
                .RuleFor(b => b.Author, f => GenerateAuthor(f, validLocale))
                .RuleFor(b => b.Publisher, f => GeneratePublisher(f, validLocale))
                .RuleFor(b => b.AverageLikes, request.AvgLikes)
                .RuleFor(b => b.AverageReviews, request.AvgReviews)
                .RuleFor(b => b.ActualLikes, f => CalculateActualLikes(f, request.AvgLikes))
                .RuleFor(b => b.Reviews, f => GenerateReviews(f, validLocale, request.AvgReviews))
                .RuleFor(b => b.Reviewers, f => GenerateReviewers(f, request.AvgReviews))
                .RuleFor(b => b.CoverImageUrl, f => GenerateCoverImageUrl(f, validLocale));

            return bookFaker.Generate(request.PageSize);
        }

        private string GenerateAuthor(Faker f, string locale)
        {
            return locale switch
            {
                "de" => $"{f.Name.LastName()}, {f.Name.FirstName()}",
                "fr" => $"{f.Name.FirstName()} {f.Name.LastName()}",
                _ => $"{f.Name.FirstName()} {f.Name.LastName()}"
            };
        }

        private int CalculateActualLikes(Faker f, double avgLikes)
        {
            var baseLikes = (int)Math.Floor(avgLikes);
            var probability = avgLikes - baseLikes;
            return baseLikes + (f.Random.Double() < probability ? 1 : 0);
        }

        private string GeneratePublisher(Faker f, string locale)
        {
            return locale switch
            {
                "de" => $"{f.Company.CompanyName()} Verlag",
                "fr" => $"Éditions {f.Company.CompanyName()}",
                _ => $"{f.Company.CompanyName()} Publishing"
            };
        }

        private List<string> GenerateReviewers(Faker f, double avgReviews)
        {
            var reviewers = new List<string>();
            var reviewCount = CalculateActualLikes(f, avgReviews);
            
            for (int i = 0; i < reviewCount; i++)
            {
                reviewers.Add(f.Name.FullName());
            }
            return reviewers;
        }

        private string GenerateTitle(Faker f, string locale)
        {
            return locale switch
            {
                "de" => GenerateGermanTitle(f),
                "fr" => GenerateFrenchTitle(f),
                _ => GenerateEnglishTitle(f)
            };
        }

        private string GenerateEnglishTitle(Faker f)
        {
            var patterns = new[]
            {
                "{Adjective} {Noun}",
                "The {Noun} of {Place}",
                "{Name}'s {Adjective} {Noun}",
                "When {Noun} {Verb}",
                "{Adjective} {Noun}: A {Noun} Story"
            };

            string[] adjectives = { "Lost", "Forgotten", "Golden", "Dark", "Mysterious", "Secret", "Ancient" };
            string[] nouns = { "Dragon", "Castle", "King", "Sword", "Journey", "Prophet", "Kingdom" };
            string[] places = { "the Mountains", "the Forest", "the River", "the Castle", "Time", "the Stars" };
            string[] verbs = { "Falls", "Rises", "Returns", "Whispers", "Burns" };

            var pattern = f.PickRandom(patterns);
            
            return pattern
                .Replace("{Adjective}", f.PickRandom(adjectives))
                .Replace("{Noun}", f.PickRandom(nouns))
                .Replace("{Place}", f.PickRandom(places))
                .Replace("{Verb}", f.PickRandom(verbs))
                .Replace("{Name}", f.Name.FirstName());
        }

        private string GenerateGermanTitle(Faker f)
        {
            var patterns = new[]
            {
                "{Adjektiv} {Nomen}",
                "Das {Nomen} von {Ort}",
                "{Name}s {Adjektiv} {Nomen}",
                "Als {Nomen} {Verb}",
                "{Adjektiv} {Nomen}: Eine {Nomen} Geschichte"
            };

            string[] adjektivs = { "Verloren", "Vergessen", "Goldene", "Dunkle", "Geheimnisvolle", "Alte" };
            string[] nomens = { "Drache", "Schloss", "König", "Schwert", "Reise", "Prophet", "Königreich" };
            string[] orts = { "den Bergen", "dem Wald", "dem Fluss", "der Burg", "der Zeit", "den Sternen" };
            string[] verbs = { "Fällt", "Steigt", "Kehrt zurück", "Flüstert", "Brennt" };

            var pattern = f.PickRandom(patterns);
            
            return pattern
                .Replace("{Adjektiv}", f.PickRandom(adjektivs))
                .Replace("{Nomen}", f.PickRandom(nomens))
                .Replace("{Ort}", f.PickRandom(orts))
                .Replace("{Verb}", f.PickRandom(verbs))
                .Replace("{Name}", f.Name.FirstName());
        }

        private string GenerateFrenchTitle(Faker f)
        {
            var patterns = new[]
            {
                "{Adjectif} {Nom}",
                "Le {Nom} de {Lieu}",
                "{Nom} {Adjectif} de {Nom}",
                "Quand {Nom} {Verbe}",
                "{Adjectif} {Nom}: Une histoire de {Nom}"
            };

            string[] adjectifs = { "Perdu", "Oublié", "Doré", "Sombre", "Mystérieux", "Secret", "Ancien" };
            string[] noms = { "Dragon", "Château", "Roi", "Épée", "Voyage", "Prophète", "Royaume" };
            string[] lieux = { "les Montagnes", "la Forêt", "la Rivière", "le Château", "le Temps", "les Étoiles" };
            string[] verbes = { "Tombe", "Monte", "Revient", "Murmure", "Brûle" };

            var pattern = f.PickRandom(patterns);
            
            return pattern
                .Replace("{Adjectif}", f.PickRandom(adjectifs))
                .Replace("{Nom}", f.PickRandom(noms))
                .Replace("{Lieu}", f.PickRandom(lieux))
                .Replace("{Verbe}", f.PickRandom(verbes))
                .Replace("{Name}", f.Name.FirstName());
        }

        private string GenerateISBN(Faker f, string locale)
        {
            // Language-specific ISBN prefixes
            var prefix = locale switch
            {
                "de" => "978-3",  // Germany
                "fr" => "978-2",  // France
                _ => "978-0"      // English-speaking countries
            };
            
            // Generate random digits
            var digits = f.Random.Digits(9).Select(d => d.ToString()).Aggregate((a, b) => a + b);
            
            // Calculate check digit
            var sum = 0;
            for (int i = 0; i < 12; i++)
            {
                var digit = int.Parse((prefix.Replace("-", "") + digits)[i].ToString());
                sum += (i % 2 == 0) ? digit : digit * 3;
            }
            var checkDigit = (10 - (sum % 10)) % 10;
            
            return $"{prefix}-{digits}-{checkDigit}";
        }

        private List<string> GenerateReviews(Faker f, string locale, double avgReviews)
        {
            var reviews = new List<string>();
            var reviewCount = CalculateActualLikes(f, avgReviews);
            var lorem = new Bogus.DataSets.Lorem(locale);
            
            for (int i = 0; i < reviewCount; i++)
            {
                reviews.Add(lorem.Paragraph());
            }
            return reviews;
        }

        private string GenerateCoverImageUrl(Faker f, string locale)
        {
            string[] colors = { "264653", "2a9d8f", "e9c46a", "f4a261", "e76f51" };
            string color = f.PickRandom(colors);
            
            // Safely get title and author
            string title = GenerateTitle(f, locale);
            string author = GenerateAuthor(f, locale);
            
            // Ensure we don't exceed string length
            string safeTitle = title.Length <= 20 ? title : title.Substring(0, 20);
            string safeAuthor = author.Length <= 20 ? author : author.Substring(0, 20);
            
            // URL encode the text
            string encodedTitle = Uri.EscapeDataString(safeTitle);
            string encodedAuthor = Uri.EscapeDataString(safeAuthor);
            
            return $"https://fakeimg.pl/300x450/{color}/ffffff/?text={encodedTitle}%0Aby {encodedAuthor}&font_size=20";
        }
    }
}