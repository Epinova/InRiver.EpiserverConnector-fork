using System.Xml.Serialization;

namespace inRiver.EPiServerCommerce.MediaPublisher.Poco
{
	
	public class EntryCode
	{
		// ATTRIBUTES
		[XmlIgnore]
		public bool IsMainPicture { get; set; }

		[XmlAttribute("IsMainPicture")]
		public string IsMainPictureString
		{
			get { return IsMainPicture ? "true" : "false"; }
			set { IsMainPicture = value == "true"; }
		}
		
		// ELEMENTS
		[XmlText]
		public string Value { get; set; }
		
		// CONSTRUCTOR
		public EntryCode()
		{}
	}
}
