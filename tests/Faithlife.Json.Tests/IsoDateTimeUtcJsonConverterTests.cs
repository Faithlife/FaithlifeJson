using System;
using Faithlife.Json.Converters;
using NUnit.Framework;
using Newtonsoft.Json;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class IsoDateTimeUtcJsonConverterTests
	{
		[Test]
		public void Elevens()
		{
			DateTime elevens = new DateTime(2011, 11, 11, 11, 11, 11, DateTimeKind.Utc);
			DateTime twelves = new DateTime(2012, 12, 12, 12, 12, 12, DateTimeKind.Utc);

			DateTimes dates = new DateTimes { Start = elevens };
			JsonConvert.SerializeObject(dates, new IsoDateTimeUtcJsonConverter()).ShouldBe(@"{""Start"":""2011-11-11T11:11:11Z"",""End"":null}");
			dates.End = twelves;
			JsonConvert.SerializeObject(dates, new IsoDateTimeUtcJsonConverter()).ShouldBe(@"{""Start"":""2011-11-11T11:11:11Z"",""End"":""2012-12-12T12:12:12Z""}");
			JsonUtility.ToJson(dates).ShouldBe(@"{""start"":""2011-11-11T11:11:11Z"",""end"":""2012-12-12T12:12:12Z""}");

			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				DateParseHandling = DateParseHandling.None,
				Converters = { new IsoDateTimeUtcJsonConverter() }
			};
			dates = JsonConvert.DeserializeObject<DateTimes>(@"{""Start"":""2011-11-11T11:11:11Z"",""End"":null}", settings);
			dates.Start.ShouldBe(elevens);
			dates.End.ShouldBe(null);
			dates = JsonConvert.DeserializeObject<DateTimes>(@"{""Start"":""2011-11-11T11:11:11Z"",""End"":""2012-12-12T12:12:12Z""}", settings);
			dates.Start.ShouldBe(elevens);
			dates.End.ShouldBe(twelves);
			dates = JsonUtility.FromJson<DateTimes>(@"{""Start"":""2011-11-11T11:11:11Z"",""End"":""2012-12-12T12:12:12Z""}");
			dates.Start.ShouldBe(elevens);
			dates.End.ShouldBe(twelves);
		}

		public class DateTimes
		{
			public DateTime Start { get; set; }
			public DateTime? End { get; set; }
		}
	}
}
