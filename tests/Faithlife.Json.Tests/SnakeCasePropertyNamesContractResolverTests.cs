using System;
using Faithlife.Json.ContractResolvers;
using NUnit.Framework;
using Newtonsoft.Json;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class SnakeCasePropertyNamesContractResolverTests
	{
		[Test]
		public void UsingStringComparisons()
		{
			var result = JsonConvert.DeserializeObject<Model>(
				@"{
					""regular_case"": 7,
					""with_acronym_at_end"": 11,
					""withacronym_at_beginning"": 13,
					""with_acronym_underscore_at_beginning"": 17,
					""with_acronym_inmiddle"": 19,
					""with_acronym_underscore_in_middle"": 21
				}",
				new JsonSerializerSettings
				{
					ContractResolver = new SnakeCasePropertyNamesContractResolver(),
				});
			result.RegularCase.ShouldBe(7);
			result.WithAcronymAtEND.ShouldBe(11);
			result.WITHAcronymAtBeginning.ShouldBe(13);
			result.WITH_AcronymUnderscoreAtBeginning.ShouldBe(17);
			result.WithAcronymINMiddle.ShouldBe(19);
			result.WithAcronymUnderscoreIN_Middle.ShouldBe(21);
		}

		// ReSharper disable InconsistentNaming
		private sealed class Model
		{
			public int RegularCase { get; set; }

			public int WithAcronymAtEND { get; set; }

			public int WITHAcronymAtBeginning { get; set; } // Proper behavior for, e.g., "OAuth" => "oauth"

			public int WITH_AcronymUnderscoreAtBeginning { get; set; }

			public int WithAcronymINMiddle { get; set; } // Proper behavior for, e.g., "OAuth" => "oauth"

			public int WithAcronymUnderscoreIN_Middle { get; set; }
		}
		// ReSharper restore InconsistentNaming
	}
}
