using System;
using SQLite;

namespace POIntillismeApp
{
	[Table("POITable")]
	public class PointOfInterest
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set;}

		[NotNull]
		public string Name { get; set; }

		[MaxLength(1000)]
		public string Description { get; set; }

		[MaxLength(150)]
		public string Address { get; set; }

		public string Image  { get; set; }
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }
	}
}
