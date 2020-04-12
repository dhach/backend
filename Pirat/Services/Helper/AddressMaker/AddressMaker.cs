﻿using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pirat.Codes;
using Pirat.Exceptions;
using Pirat.Model;
using Pirat.Model.Entity.Resource.Common;
using Pirat.Other;

namespace Pirat.Services.Helper.AddressMaker
{
	public class AddressMaker : IAddressMaker
	{
		public void SetCoordinates(AddressEntity address)
		{
            NullCheck.ThrowIfNull<AddressEntity>(address);
			string apiKey = Environment.GetEnvironmentVariable("PIRAT_GOOGLE_API_KEY");
			string addressString = address.ToString();
			Uri uri = new Uri("https://maps.googleapis.com/maps/api/geocode/json?address=" + addressString + "&key=" + apiKey);

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			string responseString;
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			using (Stream stream = response.GetResponseStream())
			using (StreamReader reader = new StreamReader(stream))
			{
				responseString = reader.ReadToEnd();
			}

			JObject json = (JObject) JsonConvert.DeserializeObject(responseString);
			JArray result = (JArray) json.GetValue("results");
			if (result.Count == 0)
			{
				throw new UnknownAdressException(FailureCodes.INVALID_ADDRESS);
			}
			JObject location = (JObject)((JObject)((JObject)result[0]).GetValue("geometry")).GetValue("location");
			decimal lat = location.GetValue("lat").ToObject<decimal>();
			decimal lng = location.GetValue("lng").ToObject<decimal>();

			address.latitude = lat;
			address.longitude = lng;
			address.hascoordinates = true;
		}
	}
}
