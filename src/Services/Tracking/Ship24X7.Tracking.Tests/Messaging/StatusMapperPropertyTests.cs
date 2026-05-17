// Feature: true-microservices-refactor, Property 3: Tracking service maps valid status strings to enum values

using FsCheck;
using FsCheck.Xunit;
using Ship24X7.Tracking.Domain.Enums;
using Ship24X7.Tracking.Infrastructure.Messaging;

namespace Ship24X7.Tracking.Tests.Messaging;

/// <summary>
/// Property-based tests for <see cref="StatusMapper"/>.
///
/// Validates: Requirements 1.3
/// </summary>
public class StatusMapperPropertyTests
{
    /// <summary>
    /// Generates a valid <see cref="ShipmentStatus"/> enum name with random casing applied to
    /// each character, so the mapper is exercised with every possible capitalisation variant.
    /// </summary>
    private static Gen<string> ValidStatusStringWithRandomCasing()
    {
        var names = Enum.GetNames<ShipmentStatus>();

        // Pick a random enum name, then randomise the casing of each character.
        return Gen.Elements(names).SelectMany(name =>
            Gen.ListOf(name.Length, Arb.Generate<bool>())
               .Select(flips =>
               {
                   var chars = new char[name.Length];
                   for (var i = 0; i < name.Length; i++)
                       chars[i] = flips[i] ? char.ToUpper(name[i]) : char.ToLower(name[i]);
                   return new string(chars);
               }));
    }

    /// <summary>
    /// Property 3: For any string that is the name of a <see cref="ShipmentStatus"/> enum member
    /// (with any casing), <see cref="StatusMapper.Parse"/> must return the corresponding enum
    /// value without throwing an exception.
    ///
    /// Validates: Requirements 1.3
    /// </summary>
    [Property(Arbitrary = new[] { typeof(ValidStatusStringArbitrary) }, MaxTest = 200)]
    public Property ValidStatusString_MapsToCorrectEnumValue(string statusString)
    {
        // Determine the expected enum value by parsing the original (canonical) name.
        // Since the input is a casing variant of a valid name, TryParse with ignoreCase
        // must succeed and return the matching value.
        var succeeded = Enum.TryParse<ShipmentStatus>(statusString, ignoreCase: true, out var expected);

        // Guard: the generator should only produce valid names, but be defensive.
        if (!succeeded)
            return true.ToProperty(); // skip invalid inputs (shouldn't happen)

        ShipmentStatus? result = null;
        Exception? thrown = null;

        try
        {
            result = StatusMapper.Parse(statusString);
        }
        catch (Exception ex)
        {
            thrown = ex;
        }

        return (thrown == null)
            .Label($"StatusMapper.Parse threw {thrown?.GetType().Name}: {thrown?.Message}")
            .And(result.HasValue)
            .Label($"StatusMapper.Parse returned null for valid status '{statusString}'")
            .And(result == expected)
            .Label($"Expected {expected} but got {result} for input '{statusString}'");
    }

    /// <summary>
    /// Registers the custom arbitrary so FsCheck can discover it via the <c>Arbitrary</c>
    /// attribute on the property test.
    /// </summary>
    public static class ValidStatusStringArbitrary
    {
        public static Arbitrary<string> StatusString() =>
            ValidStatusStringWithRandomCasing().ToArbitrary();
    }
}
