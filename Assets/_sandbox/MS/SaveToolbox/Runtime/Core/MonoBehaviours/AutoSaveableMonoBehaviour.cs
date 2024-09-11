using System.Reflection;
using SaveToolbox.Runtime.Serialization;
using SaveToolbox.Runtime.Utils;

namespace SaveToolbox.Runtime.Core.MonoBehaviours
{
	public abstract class AutoSaveableMonoBehaviour : SaveableMonoBehaviour
	{
		public override object Serialize()
		{
			var dataContainer = new StbDataContainer();
			var allSerializableFields = StbUtilities.GetAllSerializableFieldsAndProperties(GetType(), SaveToolboxSystem.Instance.CurrentSerializationSettings);
			foreach (var serializableField in allSerializableFields)
			{
				if (serializableField is FieldInfo fieldInfo)
				{
					dataContainer.AddKeyValueEntry(fieldInfo.Name, fieldInfo.GetValue(this));
				}
				else if (serializableField is PropertyInfo propertyInfo)
				{
					dataContainer.AddKeyValueEntry(propertyInfo.Name, propertyInfo.GetValue(this));
				}
			}

			return dataContainer;
		}

		public override void Deserialize(object data)
		{
			var dataContainer = (StbDataContainer)data;
			var allSerializableFields = StbUtilities.GetAllSerializableFieldsAndProperties(GetType(), SaveToolboxSystem.Instance.CurrentSerializationSettings);
			foreach (var serializableField in allSerializableFields)
			{
				if (serializableField is FieldInfo fieldInfo)
				{
					var value = dataContainer.GetValue(fieldInfo.Name);
					if (value == null) continue;

					fieldInfo.SetValue(this, value);
				}
				else if (serializableField is PropertyInfo propertyInfo)
				{
					var value = dataContainer.GetValue(propertyInfo.Name);
					if (value == null) continue;

					propertyInfo.SetValue(this, value);
				}
			}
		}
	}
}