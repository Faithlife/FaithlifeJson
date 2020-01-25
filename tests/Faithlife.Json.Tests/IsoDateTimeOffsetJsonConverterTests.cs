using System;
using Faithlife.Json.Converters;
using NUnit.Framework;
using Newtonsoft.Json;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class IsoDateTimeOffsetJsonConverterTests
	{
		[Test]
		public void TestWithoutOffset()
		{
			DateTimeOffset elevens = new DateTimeOffset(2011, 11, 11, 11, 11, 11, new TimeSpan(0, 0, 0));
			DateTimeOffset twelves = new DateTimeOffset(2012, 12, 12, 12, 12, 12, new TimeSpan(0, 0, 0));

			DateTimeOffsets dates = new DateTimeOffsets { Start = elevens };
			JsonConvert.SerializeObject(dates, new IsoDateTimeOffsetJsonConverter())
				.ShouldBe(@"{""Start"":""2011-11-11T11:11:11+00:00"",""End"":null}");

			dates.End = twelves;
			JsonConvert.SerializeObject(dates, new IsoDateTimeOffsetJsonConverter())
				.ShouldBe(@"{""Start"":""2011-11-11T11:11:11+00:00"",""End"":""2012-12-12T12:12:12+00:00""}");
			JsonUtility.ToJson(dates)
				.ShouldBe(@"{""start"":""2011-11-11T11:11:11+00:00"",""end"":""2012-12-12T12:12:12+00:00""}");

			dates = JsonConvert.DeserializeObject<DateTimeOffsets>(
				@"{""Start"":""2011-11-11T11:11:11+00:00"",""End"":null}", new IsoDateTimeOffsetJsonConverter());

			dates.Start.ShouldBe(elevens);
			dates.End.ShouldBe(null);
			dates = JsonConvert.DeserializeObject<DateTimeOffsets>(
				@"{""Start"":""2011-11-11T11:11:11+00:00"",""End"":""2012-12-12T12:12:12+00:00""}", new IsoDateTimeOffsetJsonConverter());
			dates.Start.ShouldBe(elevens);
			dates.End.ShouldBe(twelves);
			dates = JsonUtility.FromJson<DateTimeOffsets>(
				@"{""Start"":""2011-11-11T11:11:11+00:00"",""End"":""2012-12-12T12:12:12+00:00""}")!;
			dates.Start.ShouldBe(elevens);
			dates.End.ShouldBe(twelves);
		}

		[Test]
		public void TestWithOffset()
		{
			DateTimeOffset elevens = new DateTimeOffset(2011, 11, 11, 11, 11, 11, new TimeSpan(5, 0, 0));
			DateTimeOffset twelves = new DateTimeOffset(2012, 12, 12, 12, 12, 12, new TimeSpan(5, 0, 0));

			DateTimeOffsets dates = new DateTimeOffsets { Start = elevens };
			JsonConvert.SerializeObject(dates, new IsoDateTimeOffsetJsonConverter())
				.ShouldBe(@"{""Start"":""2011-11-11T11:11:11+05:00"",""End"":null}");

			dates.End = twelves;
			JsonConvert.SerializeObject(dates, new IsoDateTimeOffsetJsonConverter())
				.ShouldBe(@"{""Start"":""2011-11-11T11:11:11+05:00"",""End"":""2012-12-12T12:12:12+05:00""}");

			dates = JsonConvert.DeserializeObject<DateTimeOffsets>(
				@"{""Start"":""2011-11-11T11:11:11+05:00"",""End"":null}", new IsoDateTimeOffsetJsonConverter());

			dates.Start.ShouldBe(elevens);
			dates.End.ShouldBe(null);
			dates = JsonConvert.DeserializeObject<DateTimeOffsets>(
				@"{""Start"":""2011-11-11T11:11:11+05:00"",""End"":""2012-12-12T12:12:12+05:00""}", new IsoDateTimeOffsetJsonConverter());
			dates.Start.ShouldBe(elevens);
			dates.End.ShouldBe(twelves);
		}

		[Test]
		public void SecondsRequired()
		{
			Assert.Throws<JsonSerializationException>(() => JsonUtility.FromJson<DateTimeOffset>(@"""2011-11-11T11:11+05:00"""));
		}

		public class DateTimeOffsets
		{
			public DateTimeOffset Start { get; set; }
			public DateTimeOffset? End { get; set; }
		}
	}
}
