using System;
using Faithlife.Json.Converters;
using NUnit.Framework;
using Newtonsoft.Json;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class GuidLowerNoDashJsonConverterTests
	{
		[Test]
		public void SaveAndLoad()
		{
			Guid start = new Guid("5f346a0d-a38b-40c2-9289-44c5ffe40752");
			Guid end = new Guid("4ea3a3fd-8892-46b5-8dca-370e64f6c4f9");

			Guids guids = new Guids { Start = start };
			JsonConvert.SerializeObject(guids, new GuidLowerNoDashJsonConverter()).ShouldBe(@"{""Start"":""5f346a0da38b40c2928944c5ffe40752"",""End"":null}");
			guids.End = end;
			JsonConvert.SerializeObject(guids, new GuidLowerNoDashJsonConverter()).ShouldBe(@"{""Start"":""5f346a0da38b40c2928944c5ffe40752"",""End"":""4ea3a3fd889246b58dca370e64f6c4f9""}");

			guids = JsonConvert.DeserializeObject<Guids>(@"{""Start"":""5f346a0da38b40c2928944c5ffe40752"",""End"":null}", new GuidLowerNoDashJsonConverter());
			guids.Start.ShouldBe(start);
			guids.End.ShouldBe(null);
			guids = JsonConvert.DeserializeObject<Guids>(@"{""Start"":""5f346a0da38b40c2928944c5ffe40752"",""End"":""4ea3a3fd889246b58dca370e64f6c4f9""}", new GuidLowerNoDashJsonConverter());
			guids.Start.ShouldBe(start);
			guids.End.ShouldBe(end);
		}

		[Test]
		public void Fail()
		{
			Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Guids>(@"{""Start"":""5f346a0d-a38b-40c2-9289-44c5ffe40752"",""End"":null}", new GuidLowerNoDashJsonConverter()));
			Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Guids>(@"{""Start"":""5F346a0da38b40c2928944c5ffe40752"",""End"":null}", new GuidLowerNoDashJsonConverter()));
		}

		public class Guids
		{
			public Guid Start { get; set; }
			public Guid? End { get; set; }
		}
	}
}
