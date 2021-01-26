using System;
using Faithlife.Json.Converters;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class IsoDateOnlyJsonConverterTests
	{
		[Test]
		public void Elevens()
		{
			DateTime elevens = new DateTime(2011, 11, 11, 0, 0, 0, DateTimeKind.Utc);
			DateTime twelves = new DateTime(2012, 12, 12, 0, 0, 0, DateTimeKind.Utc);

			DateTimes dates = new DateTimes { Start = elevens };
			JsonConvert.SerializeObject(dates, new IsoDateOnlyJsonConverter()).ShouldBe(@"{""Start"":""2011-11-11"",""End"":null}");
			dates.End = twelves;
			JsonConvert.SerializeObject(dates, new IsoDateOnlyJsonConverter()).ShouldBe(@"{""Start"":""2011-11-11"",""End"":""2012-12-12""}");
			JsonUtility.ToJson(dates, new JsonSettings { Converters = new[] { new IsoDateOnlyJsonConverter() } }).ShouldBe(@"{""start"":""2011-11-11"",""end"":""2012-12-12""}");

			dates = JsonConvert.DeserializeObject<DateTimes>(@"{""Start"":""2011-11-11"",""End"":null}", new IsoDateOnlyJsonConverter());
			dates.Start.ShouldBe(elevens);
			dates.End.ShouldBe(null);
			dates = JsonConvert.DeserializeObject<DateTimes>(@"{""Start"":""2011-11-11"",""End"":""2012-12-12""}", new IsoDateOnlyJsonConverter());
			dates.Start.ShouldBe(elevens);
			dates.End.ShouldBe(twelves);
			dates = JsonUtility.FromJson<DateTimes>(@"{""Start"":""2011-11-11"",""End"":""2012-12-12""}", new JsonSettings { Converters = new[] { new IsoDateOnlyJsonConverter() } })!;
			dates.Start.ShouldBe(elevens);
			dates.End.ShouldBe(twelves);
		}

		private class DateTimes
		{
			public DateTime Start { get; set; }
			public DateTime? End { get; set; }
		}
	}
}
