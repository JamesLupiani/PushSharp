using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PushSharp.Windows
{
	public abstract class WindowsNotification : Common.Notification
	{
		protected WindowsNotification()
		{
			this.Platform = Common.PlatformType.Windows;
		}

		public string ChannelUri { get; set; }

		public bool? RequestForStatus { get; set; }
		public int? TimeToLive { get; set; }
		
		public abstract string PayloadToString();

		public abstract WindowsNotificationType Type { get; }

		protected string XmlEncode(string text)
		{
			return System.Security.SecurityElement.Escape(text);
		}

		protected string GeneratePayload(XElement rootElement, string template, Dictionary<string, string> images, List<string> texts)
		{
			var visual = new XElement("visual");
			var binding = new XElement("binding", new XAttribute("template", template.ToString()));

			int idOn = 1;

			foreach (var imgSrc in images.Keys)
			{
				var alt = images[imgSrc];

				var image = new XElement("image", new XAttribute("id", idOn), new XAttribute("src", XmlEncode(imgSrc)));

				if (!string.IsNullOrEmpty(alt))
					image.Add(new XAttribute("alt", XmlEncode(alt)));

				binding.Add(image);

				idOn++;
			}

			idOn = 1;

			foreach (var text in texts)
			{
				binding.Add(new XElement("text", new XAttribute("id", idOn), XmlEncode(text)));
				idOn++;
			}

			visual.Add(binding);
			rootElement.Add(visual);
			return rootElement.ToString();
		}
	}

	public class WindowsTileNotification : WindowsNotification
	{
		public WindowsTileNotification()
			: base()
		{
			this.Texts = new List<string>();
			this.Images = new Dictionary<string, string>();
			this.TileTemplate = TileNotificationTemplate.TileSquareBlock;
		}

		public override WindowsNotificationType Type
		{
			get { return WindowsNotificationType.Tile; }
		}

		public WindowsNotificationCachePolicyType? CachePolicy { get; set; }
		public string NotificationTag { get; set; }

		public TileNotificationTemplate TileTemplate { get; set; }
		public Dictionary<string, string> Images { get; set; }
		public List<string> Texts { get; set; }

		public override string PayloadToString()
		{
			return this.GeneratePayload(new XElement("tile"), this.TileTemplate.ToString(), Images, Texts);
		}
	}

	public class WindowsToastNotification : WindowsNotification
	{
		public WindowsToastNotification()
			: base()
		{
			this.Texts = new List<string>();
			this.Images = new Dictionary<string, string>();
			this.TextTemplate = ToastNotificationTemplate.ToastImageAndText01;
		}

		public override WindowsNotificationType Type
		{
			get { return WindowsNotificationType.Toast; }
		}

		public string Launch { get; set; }
		public ToastNotificationDuration Duration { get; set; }
		public ToastNotificationTemplate TextTemplate { get; set; }
		public Dictionary<string, string> Images { get; set; }
		public List<string> Texts { get; set; }

		public override string PayloadToString()
		{
			var rootElement = new XElement("toast");

			if (!string.IsNullOrEmpty(Launch))
				rootElement.Add(new XAttribute("launch", XmlEncode(Launch)));

			if (Duration != ToastNotificationDuration.Short)
				rootElement.Add(new XAttribute("duration", "long"));

			return this.GeneratePayload(rootElement, this.TextTemplate.ToString(), Images, Texts);
		}
	}

	public class WindowsBadgeNotification : WindowsNotification
	{
		public override WindowsNotificationType Type
		{
			get { return WindowsNotificationType.Badge; }
		}

		public WindowsNotificationCachePolicyType? CachePolicy { get; set; }

		public int? Numeric { get; set; }
		public BadgeNotificationGlyph? Glyph { get; set; }

		public override string PayloadToString()
		{
			if (!Numeric.HasValue && !Glyph.HasValue)
				throw new InvalidOperationException("Either a numeric or glyph value is required.");

			var xml = new StringBuilder();

			var valueString = string.Empty;

			if (Numeric.HasValue)
				valueString = Numeric.Value.ToString();
			else if (Glyph.HasValue)
			{
				switch (Glyph.Value)
				{
					case BadgeNotificationGlyph.None:
						valueString = "none";
						break;

					case BadgeNotificationGlyph.Activity:
						valueString = "activity";
						break;

					case BadgeNotificationGlyph.Alert:
						valueString = "alert";
						break;

					case BadgeNotificationGlyph.Available:
						valueString = "available";
						break;

					case BadgeNotificationGlyph.Away:
						valueString = "away";
						break;

					case BadgeNotificationGlyph.Busy:
						valueString = "busy";
						break;

					case BadgeNotificationGlyph.NewMessage:
						valueString = "newMessage";
						break;

					case BadgeNotificationGlyph.Paused:
						valueString = "paused";
						break;

					case BadgeNotificationGlyph.Playing:
						valueString = "playing";
						break;

					case BadgeNotificationGlyph.Unavailable:
						valueString = "unavailable";
						break;

					case BadgeNotificationGlyph.Error:
						valueString = "error";
						break;

					case BadgeNotificationGlyph.Attention:
						valueString = "attention";
						break;
				}
			}

			xml.AppendFormat("<badge value=\"{0}\">", valueString);

			return xml.ToString();
		}
	}

	public class WindowsRawNotification : WindowsNotification
	{
		public override WindowsNotificationType Type
		{
			get { return WindowsNotificationType.Raw; }
		}

		public string RawXml { get; set; }

		public override string PayloadToString()
		{
			return RawXml;
		}
	}

	public enum WindowsNotificationCachePolicyType
	{
		Cache,
		NoCache
	}

	public enum WindowsNotificationType
	{
		Badge,
		Tile,
		Toast,
		Raw
	}

	public enum ToastNotificationTemplate
	{
		ToastText01,
		ToastText02,
		ToastText03,
		ToastText04,
		ToastImageAndText01,
		ToastImageAndText02,
		ToastImageAndText03,
		ToastImageAndText04
	}

	public enum ToastNotificationDuration
	{
		Short,
		Long
	}

	public enum BadgeNotificationGlyph
	{
		None,
		Activity,
		Alert,
		Available,
		Away,
		Busy,
		NewMessage,
		Paused,
		Playing,
		Unavailable,
		Error,
		Attention
	}
}
