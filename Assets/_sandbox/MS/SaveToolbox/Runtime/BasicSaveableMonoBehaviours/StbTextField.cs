using System;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
	/// <summary>
	/// Saves data about the Text component that is referenced. Saves data such as font size as well the text that is in the component.
	/// </summary>
	[AddComponentMenu("SaveToolbox/SavingBehaviours/StbTextField")]
	public class StbTextField : SaveableMonoBehaviour
	{
		[SerializeField]
		private Text textField;

		public override object Serialize()
		{
			if (textField == null)
			{
				if (!TryGetComponent(out textField)) throw new Exception($"Could not serialize object of type textField as there isn't one referenced or attached to the game object.");
			}
			return new TextFieldSaveData(textField);
		}

		public override void Deserialize(object data)
		{
			if (textField == null)
			{
				if (!TryGetComponent(out textField)) throw new Exception($"Could not deserialize object of type textField as there isn't one referenced or attached to the game object.");
			}
			var castData = (TextFieldSaveData)data;
			textField.fontStyle = (FontStyle)castData.FontStyle;
			textField.fontSize = castData.FontSize;
			textField.lineSpacing = castData.FontSize;
			textField.supportRichText = castData.RichText;
			textField.alignment = (TextAnchor)castData.Alignment;
			textField.alignByGeometry = castData.AlignByGeometry;
			textField.horizontalOverflow = (HorizontalWrapMode)castData.HorizontalOverflow;
			textField.verticalOverflow = (VerticalWrapMode)castData.VerticalOverflow;
			textField.resizeTextForBestFit = castData.BestFit;
			textField.color = castData.Color;
			textField.raycastTarget = castData.RaycastTarget;
#if STB_ABOVE_2021_3
			textField.raycastPadding = castData.RaycastPadding;
#endif
			textField.maskable = castData.Maskable;
		}
	}

	[Serializable]
	public struct TextFieldSaveData
	{
		[SerializeField, StbSerialize]
		private int fontStyle;
		public int FontStyle => fontStyle;

		[SerializeField, StbSerialize]
		private int fontSize;
		public int FontSize => fontSize;

		[SerializeField, StbSerialize]
		private float lineSpacing;
		public float LineSpacing => lineSpacing;

		[SerializeField, StbSerialize]
		private bool richText;
		public bool RichText => richText;

		[SerializeField, StbSerialize]
		private int alignment;
		public int Alignment => alignment;

		[SerializeField, StbSerialize]
		private bool alignByGeometry;
		public bool AlignByGeometry => alignByGeometry;

		[SerializeField, StbSerialize]
		private int horizontalOverflow;
		public int HorizontalOverflow => horizontalOverflow;

		[SerializeField, StbSerialize]
		private int verticalOverflow;
		public int VerticalOverflow => verticalOverflow;

		[SerializeField, StbSerialize]
		private bool bestFit;
		public bool BestFit => bestFit;

		[SerializeField, StbSerialize]
		private Color color;
		public Color Color => color;

		[SerializeField, StbSerialize]
		private bool raycastTarget;
		public bool RaycastTarget => raycastTarget;

		[SerializeField, StbSerialize]
		private Vector4 raycastPadding;
		public Vector4 RaycastPadding => raycastPadding;

		[SerializeField, StbSerialize]
		private bool maskable;
		public bool Maskable => maskable;

		public TextFieldSaveData(Text text)
		{
			fontStyle = (int)text.fontStyle;
			fontSize = text.fontSize;
			lineSpacing = text.lineSpacing;
			richText = text.supportRichText;
			alignment = (int)text.alignment;
			alignByGeometry = text.alignByGeometry;
			horizontalOverflow = (int)text.horizontalOverflow;
			verticalOverflow = (int)text.verticalOverflow;
			bestFit = text.resizeTextForBestFit;
			color = text.color;
			raycastTarget = text.raycastTarget;
#if STB_ABOVE_2021_3
			raycastPadding = text.raycastPadding;
#else
			raycastPadding = new Vector4();
#endif
			maskable = text.maskable;
		}

		public TextFieldSaveData(int fontStyle, int fontSize, float lineSpacing, bool richText, int alignment, bool alignByGeometry, int horizontalOverflow, int verticalOverflow, bool bestFit, Color color, bool raycastTarget, Vector4 raycastPadding, bool maskable)
		{
			this.fontStyle = fontStyle;
			this.fontSize = fontSize;
			this.lineSpacing = lineSpacing;
			this.richText = richText;
			this.alignment = alignment;
			this.alignByGeometry = alignByGeometry;
			this.horizontalOverflow = horizontalOverflow;
			this.verticalOverflow = verticalOverflow;
			this.bestFit = bestFit;
			this.color = color;
#if STB_ABOVE_2021_3
			this.raycastPadding = raycastPadding;
#else
			this.raycastPadding = new Vector4();
#endif
			this.raycastTarget = raycastTarget;
			this.raycastPadding = raycastPadding;
			this.maskable = maskable;
		}

	}
}