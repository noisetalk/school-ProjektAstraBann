using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;
using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Runtime.CustomComponentSavers
{
	public class TextFieldComponentSaver : AbstractComponentSaver<Text>
	{
		public override object Serialize()
		{
			return new TextFieldSaveData(Target);
		}

		public override void Deserialize(object data)
		{
			var castData = (TextFieldSaveData)data;
			Target.fontStyle = (FontStyle)castData.FontStyle;
			Target.fontSize = castData.FontSize;
			Target.lineSpacing = castData.FontSize;
			Target.supportRichText = castData.RichText;
			Target.alignment = (TextAnchor)castData.Alignment;
			Target.alignByGeometry = castData.AlignByGeometry;
			Target.horizontalOverflow = (HorizontalWrapMode)castData.HorizontalOverflow;
			Target.verticalOverflow = (VerticalWrapMode)castData.VerticalOverflow;
			Target.resizeTextForBestFit = castData.BestFit;
			Target.color = castData.Color;
			Target.raycastTarget = castData.RaycastTarget;
#if STB_ABOVE_2021_3
			Target.raycastPadding = castData.RaycastPadding;
#endif
			Target.maskable = castData.Maskable;
		}
	}
}