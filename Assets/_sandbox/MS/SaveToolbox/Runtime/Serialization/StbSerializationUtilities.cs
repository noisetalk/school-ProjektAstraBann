using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using SaveToolbox.Runtime.Serialization.Json;
using UnityEngine;

namespace SaveToolbox.Runtime.Serialization
{
	/// <summary>
	/// A static utils class in Save Toolbox that provides various helpful functions.
	/// </summary>
	public static class StbSerializationUtilities
	{
		private const string STB_ASSEMBLY_TYPE_NAME = "StbTypeAssembly";

		private static float currentFrameTime;
		private static float startFrameTime;

		private static bool hasCachedTypes;

		public static List<FormerlySerializedAsDataContainer> dataContainers = new List<FormerlySerializedAsDataContainer>();
		private static readonly string[] nonUserAssemblyNames = new[] { "System.", "UnityEngine", "UnityEditor", "Unity.", "netstandard", "Microsoft", "Mono", "mscorlib", "System", "unityplastic", "ExCSS.Unity", "Bee", "PsdPlugin", "ScriptCompilationBuildProgram" };

		/// <summary>
		/// Is the type a serializable list?
		/// </summary>
		/// <param name="type">The type that is to be checked if it serializable list.</param>
		/// <returns>If it is a serializable list.</returns>
		public static bool IsList(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
		}

		/// <summary>
		/// Is the type a serializable queue?
		/// </summary>
		/// <param name="type">The type that is to be checked if it serializable queue.</param>
		/// <returns>If it is a serializable queue.</returns>
		public static bool IsQueue(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Queue<>);
		}

		/// <summary>
		/// Is the type a serializable stack?
		/// </summary>
		/// <param name="type">The type that is to be checked if it serializable stack.</param>
		/// <returns>If it is a serializable queue.</returns>
		public static bool IsStack(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Stack<>);
		}

		public static bool IsSerializableCollection(this Type type)
		{
			return type.IsGenericType &&
			       (type.GetGenericTypeDefinition() == typeof(List<>) ||
			        type.GetGenericTypeDefinition() == typeof(Queue<>) ||
			        type.GetGenericTypeDefinition() == typeof(Stack<>));
		}

		public static bool IsSet(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SortedSet<>);
		}

		/// <summary>
		/// Applies depth for the JSON serializer.
		/// </summary>
		/// <param name="stringBuilder">The json serializer string builder.</param>
		/// <param name="depth">The current depth.</param>
		public static void ApplyDepth(StringBuilder stringBuilder, int depth)
		{
			for (var i = 0; i < depth; ++i)
			{
				stringBuilder.Append("\t");
			}
		}

		/// <summary>
		/// Attempts to get the former type of a StbFormerlySerializedAs class/struct in the form of a string.
		/// </summary>
		/// <param name="type">Current type.</param>
		/// <param name="typeString">Former type as a string.</param>
		/// <returns>If it was successful</returns>
		public static bool TryGetFormerTypeString(Type type, out string typeString)
		{
			typeString = null;

			var allFormerlySerializedAs = FindAllFormerlySerializedAsClasses();

			foreach (var dataContainer in allFormerlySerializedAs)
			{
				if (dataContainer.targetType == type)
				{
					typeString = dataContainer.formerlySerializedAsType;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Attempts to get the former type of a StbFormerlySerializedAs class/struct.
		/// </summary>
		/// <param name="typeString">Current type.</param>
		/// <param name="type">Former type.</param>
		/// <returns>if it was successful</returns>
		public static bool TryGetFormerType(string typeString, out Type type)
		{
			type = null;

			var allFormerlySerializedAs = FindAllFormerlySerializedAsClasses();

			foreach (var dataContainer in allFormerlySerializedAs)
			{
				if (dataContainer.formerlySerializedAsType == typeString)
				{
					type = dataContainer.targetType;
					return true;
				}
			}

			return false;
		}

		public static Type GetType(string typeString)
		{
			if (TryGetFormerType(typeString, out var type))
			{
				return type;
			}

			type = Type.GetType(typeString);

			if (type == null) throw new Exception($"Failed to find type at assembly : {typeString}. Have assemblies changed?");

			return type;
		}

		/// <summary>
		/// Returns all formerly serializable types. Inside of custom containers.
		/// </summary>
		/// <returns></returns>
		static List<FormerlySerializedAsDataContainer> FindAllFormerlySerializedAsClasses()
		{
			if (hasCachedTypes) return dataContainers;

			CacheFormerTypes();
			return dataContainers;
		}

		public static void CacheFormerTypes()
		{
			// Get all types in the current assembly
			var allTypes = new List<Type>();
			List<Assembly> assemblies = new List<Assembly>();
			assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !assembly.IsSystemAssembly()));

			foreach (var assembly in assemblies)
			{
				if (assembly == null) continue;
				allTypes.AddRange(assembly.GetTypes());
			}

			// Filter types that have the specified attribute
			var classesWithAttribute = allTypes
				.Where(type => type.GetCustomAttributes(typeof(StbFormerlySerializedAs), true).Length > 0);

			dataContainers = new List<FormerlySerializedAsDataContainer>();

			foreach (var type in classesWithAttribute)
			{
				var customAttributes = type.GetCustomAttributes<StbFormerlySerializedAs>();
				foreach (var customAttribute in customAttributes)
				{
					dataContainers.Add(new FormerlySerializedAsDataContainer(type, customAttribute.formerNamespace));
				}
			}

			hasCachedTypes = true;
		}

		private static bool IsSystemAssembly(this Assembly assembly)
		{
			var name = assembly.GetName().Name;
			foreach (var nonUserAssemblyName in nonUserAssemblyNames)
			{
				if (name.StartsWith(nonUserAssemblyName)) return true;
			}

			return false;
		}

		public struct FormerlySerializedAsDataContainer
		{
			public Type targetType;
			public string formerlySerializedAsType;

			public FormerlySerializedAsDataContainer(Type targetType, string formerlySerializedAsType)
			{
				this.targetType = targetType;
				this.formerlySerializedAsType = formerlySerializedAsType;
			}
		}

		public static long AsLong(this JsonSimpleNode node) =>  Convert.ToInt64(node.Value);
		public static ulong AsUlong(this JsonSimpleNode node) =>  Convert.ToUInt64(node.Value);
		public static int AsInt(this JsonSimpleNode node) =>  Convert.ToInt32(node.Value);
		public static uint AsUint(this JsonSimpleNode node) =>  Convert.ToUInt32(node.Value);
		public static short AsShort(this JsonSimpleNode node) =>  Convert.ToInt16(node.Value);
		public static ushort AsUshort(this JsonSimpleNode node) =>  Convert.ToUInt16(node.Value);
		public static decimal AsDecimal(this JsonSimpleNode node) =>  Convert.ToDecimal(node.Value);
		public static bool AsBool(this JsonSimpleNode node) =>  Convert.ToBoolean(node.Value);
		public static byte AsByte(this JsonSimpleNode node) =>  Convert.ToByte(node.Value);
		public static char AsChar(this JsonSimpleNode node) =>  Convert.ToChar(node.Value);
		public static float AsFloat(this JsonSimpleNode node) =>  Convert.ToSingle(node.Value);
		public static double AsDouble(this JsonSimpleNode node) =>  Convert.ToDouble(node.Value);
		public static sbyte AsSbyte(this JsonSimpleNode node) =>  Convert.ToSByte(node.Value);
		public static string AsString(this JsonSimpleNode node) =>  Convert.ToString(node.Value);

		internal static long AsLong(this object value) =>  Convert.ToInt64(value);
		internal static ulong AsUlong(this object value) =>  Convert.ToUInt64(value);
		internal static int AsInt(this object value) =>  Convert.ToInt32(value);
		internal static uint AsUint(this object value) =>  Convert.ToUInt32(value);
		internal static short AsShort(this object value) =>  Convert.ToInt16(value);
		internal static ushort AsUshort(this object value) =>  Convert.ToUInt16(value);
		internal static decimal AsDecimal(this object value) =>  Convert.ToDecimal(value);
		internal static bool AsBool(this object value) =>  Convert.ToBoolean(value);
		internal static byte AsByte(this object value) =>  Convert.ToByte(value);
		internal static char AsChar(this object value) =>  Convert.ToChar(value);
		internal static float AsFloat(this object value) =>  Convert.ToSingle(value);
		internal static double AsDouble(this object value) =>  Convert.ToDouble(value);
		internal static sbyte AsSbyte(this object value) =>  Convert.ToSByte(value);
		internal static string AsString(this object value) =>  Convert.ToString(value);

		/// <summary>
		/// Converts an object to the correct primitive if possible.
		/// </summary>
		/// <param name="valueToConvert">The object to try convert to a primitive.</param>
		/// <param name="type">The type of the object.</param>
		/// <returns>The converted object, if it was successful.</returns>
		public static object ConvertPrimitive(this object valueToConvert, Type type)
		{
			if (type == typeof(long)) return Convert.ToInt64(valueToConvert);
			if (type == typeof(ulong)) return Convert.ToUInt64(valueToConvert);
			if (type == typeof(int)) return Convert.ToInt32(valueToConvert);
			if (type == typeof(uint)) return Convert.ToUInt32(valueToConvert);
			if (type == typeof(short)) return Convert.ToInt16(valueToConvert);
			if (type == typeof(ushort)) return Convert.ToUInt16(valueToConvert);
			if (type == typeof(decimal)) return Convert.ToDecimal(valueToConvert);
			if (type == typeof(bool)) return Convert.ToBoolean(valueToConvert);
			if (type == typeof(byte)) return Convert.ToByte(valueToConvert);
			if (type == typeof(char)) return Convert.ToChar(valueToConvert);
			if (type == typeof(float)) return Convert.ToSingle(valueToConvert);
			if (type == typeof(double)) return Convert.ToDouble(valueToConvert);
			if (type == typeof(sbyte)) return Convert.ToSByte(valueToConvert);
			if (type == typeof(string)) return Convert.ToString(valueToConvert).Trim('"');
			if (type.IsEnum)
			{
				return Enum.ToObject(type, Convert.ToInt32(valueToConvert));
			}

			return valueToConvert;
		}

		/// <summary>
		/// Checks if a type is a primitive.
		/// </summary>
		/// <param name="type">The type to be checked if it is primitive.</param>
		/// <returns>If it is primitive.</returns>
		public static bool IsPrimitiveType(Type type)
		{
			if (type == typeof(bool)) return true;
			if (type == typeof(string)) return true;
			if (type == typeof(char)) return true;
			if (type == typeof(float)) return true;
			if (type == typeof(double)) return true;
			if (type == typeof(decimal)) return true;
			if (type == typeof(int)) return true;
			if (type == typeof(uint)) return true;
			if (type == typeof(long)) return true;
			if (type == typeof(ulong)) return true;
			if (type == typeof(short)) return true;
			if (type == typeof(ushort)) return true;
			if (type == typeof(byte)) return true;
			if (type == typeof(sbyte)) return true;

			// Array types.
			if (type == typeof(bool[])) return true;
			if (type == typeof(string[])) return true;
			if (type == typeof(char[])) return true;
			if (type == typeof(float[])) return true;
			if (type == typeof(double[])) return true;
			if (type == typeof(decimal[])) return true;
			if (type == typeof(int[])) return true;
			if (type == typeof(uint[])) return true;
			if (type == typeof(long[])) return true;
			if (type == typeof(ulong[])) return true;
			if (type == typeof(short[])) return true;
			if (type == typeof(ushort[])) return true;
			if (type == typeof(byte[])) return true;
			if (type == typeof(sbyte[])) return true;

			return false;
		}

		internal static List<JsonBaseNode> CreateNodesFromData(string jsonText, int stringIndex, bool typeSerialization = true, bool hasSingularLabel = false)
		{
			var nodeList = new List<JsonBaseNode>();
			var hasAttemptedToGetLabel = false;
			var label = string.Empty;

			for (var i = stringIndex; i < jsonText.Length; ++i)
			{
				// Try and get a label.
				if (!hasAttemptedToGetLabel)
				{
					hasAttemptedToGetLabel = true;
					var prevIndex = i;
					if (!TryGetNodeLabel(jsonText, ref i, out label))
					{
						i = prevIndex;
					}
					else
					{
						// If the whole thing is a label?
						if (i >= jsonText.Length) break;

						if (jsonText[i] == '"') i++;
					}
				}

				var tokenChar = jsonText[i];
				switch (tokenChar)
				{
					// Handle non-simple nodes without labels.
					case '{':
						if (!hasSingularLabel)
						{
							hasAttemptedToGetLabel = false;
						}

						var objectNode = CreateObjectNodeFromJson(jsonText, ref i, typeSerialization, label);
						objectNode.Name = label;
						nodeList.Add(objectNode);
						break;
					case '[':
						if (!hasSingularLabel)
						{
							hasAttemptedToGetLabel = false;
						}

						var arrayNode = CreateArrayNodeFromJson(jsonText, ref i, typeSerialization, label);
						arrayNode.Name = label;
						nodeList.Add(arrayNode);
						break;
					// Handle node values.
					case ':':
					case ',':
					case ' ':
					case '\t':
					case '\n':
						break;
					default:
					{
						if (!hasSingularLabel)
						{
							hasAttemptedToGetLabel = false;
						}

						if (TryCreateSimpleNodeFromJson(jsonText, ref i, label, out var simpleNode))
						{
							nodeList.Add(simpleNode);
						}

						break;
					}
				}
			}

			return nodeList;
		}

		internal static JsonBaseNode CreateNextNodeFromData(string jsonText, ref int stringIndex, bool typeSerialization = true)
		{
			var hasAttemptedToGetLabel = false;
			var label = string.Empty;

			for (; stringIndex < jsonText.Length; ++stringIndex)
			{
				// Try and get a label.
				if (!hasAttemptedToGetLabel)
				{
					hasAttemptedToGetLabel = true;
					var prevIndex = stringIndex;
					if (!TryGetNodeLabel(jsonText, ref stringIndex, out label))
					{
						stringIndex = prevIndex;
					}
					else
					{
						// If the whole thing is a label?
						if (stringIndex >= jsonText.Length) break;

						if (jsonText[stringIndex] == '"') stringIndex++;
					}
				}

				var tokenChar = jsonText[stringIndex];
				switch (tokenChar)
				{
					// Handle non-simple nodes without labels.
					case '{':
						var objectNode = CreateObjectNodeFromJson(jsonText, ref stringIndex, typeSerialization, label);
						objectNode.Name = label;
						return objectNode;
					case '[':
						var arrayNode = CreateArrayNodeFromJson(jsonText, ref stringIndex, typeSerialization, label);
						arrayNode.Name = label;
						return arrayNode;
					// Handle node values.
					case ':':
					case ',':
					case ' ':
					case '\t':
					case '\n':
						break;
					default:
					{
						if (TryCreateSimpleNodeFromJson(jsonText, ref stringIndex, label, out var simpleNode))
						{
							return simpleNode;
						}

						break;
					}
				}
			}

			return null;
		}

		private static JsonCompoundNode CreateArrayNodeFromJson(string jsonText, ref int i, bool typeSerialization, string label)
		{
			var startIndex = ++i;
			var endIndex = jsonText.Length - 1;
			var depth = 0;

			for (; i < jsonText.Length; ++i)
			{
				var newChar = jsonText[i];
				if (newChar.Equals('['))
				{
					++depth;
					continue;
				}

				if (newChar.Equals(']'))
				{
					if (depth == 0)
					{
						endIndex = i;
						break;
					}

					--depth;
				}
			}

			endIndex -= startIndex;
			var subString = jsonText.Substring(startIndex, endIndex);


			var currentIndex = 0;
			var isPrimitiveNode = IsNextNodePrimitiveArrayIndicator(subString, ref currentIndex);
			if (isPrimitiveNode)
			{
				return CreatePrimitiveArrayNode(subString, ref currentIndex, typeSerialization);
			}

			// Not a primitive node? reset current index.
			currentIndex = 0;
			var arrayNode = new JsonArrayNode()
			{
				Name = label
			};

			var newNodes = CreateNodesFromData(subString, currentIndex, typeSerialization, true);
			for (var index = 0; index < newNodes.Count; ++index)
			{
				var newNode = newNodes[index];
				// Handle type differently.
				if (index == 0 && newNode is JsonObjectNode objectNode && !string.IsNullOrEmpty(objectNode.Type))
				{
					arrayNode.Type = objectNode.Type;
				}
				else
				{
					arrayNode.AddChild(newNode);
				}
			}
			return arrayNode;
		}

		private static bool IsNextNodePrimitiveArrayIndicator(string jsonText, ref int i)
		{
			var foundStart = false;
			var startIndex = 0;

			for (; i < jsonText.Length; ++i)
			{
				var newChar = jsonText[i];
				switch (newChar)
				{
					case '{':
						// started, now we want to find the next } to substring it then we can Contains() it.
						foundStart = true;
						startIndex = i;
						break;
					case '"':
						if (foundStart) break;
						return false;
					case '}':
						if (foundStart)
						{
							// substring it to contains check it.
							var substring = jsonText.Substring(startIndex, i - startIndex);
							return substring.Contains("IsStbPrimitiveArray");
						}
						return false;
					case '[':
					case ']':
						return false;
				}
			}

			return false;
		}

		private static JsonPrimitiveArrayNode CreatePrimitiveArrayNode(string jsonText, ref int i, bool typeSerialization = true)
		{
			ClearAllNextUnusableCharacters(jsonText, ref i);
			var primitiveArrayNode = new JsonPrimitiveArrayNode();
			var lastInvalidCharIndex = 0;
			var isInsideString = false;
			var foundValueStrings = new List<object>();
			for (; i < jsonText.Length; ++i)
			{
				var newChar = jsonText[i];
				switch (newChar)
				{
					case '{':
						if (isInsideString) break;
						// If it finds the start of an object it must be the type indicator.
						++i;
						var typeNode = (JsonSimpleNode)CreateNextNodeFromData(jsonText, ref i, typeSerialization);
						primitiveArrayNode.Type = (string)typeNode.Value;
						ClearAllNextUnusableCharacters(jsonText, ref i);
						break;
					case '\n':
					case ',':
					case ']':
						if (isInsideString) break;
						foundValueStrings.Add(jsonText.Substring(lastInvalidCharIndex + 1, i - (lastInvalidCharIndex + 1)));
						ClearAllNextUnusableCharacters(jsonText, ref i);
						break;
					case ' ':
					case '\t':
						if (isInsideString) break;
						lastInvalidCharIndex = i;
						break;
					case '"':
						if (!isInsideString)
						{
							lastInvalidCharIndex = i;
						}
						isInsideString = !isInsideString;
						// If we just escaped a string then that is our value
						if (!isInsideString)
						{
							foundValueStrings.Add(jsonText.Substring(lastInvalidCharIndex + 1, i - (lastInvalidCharIndex + 1)));
							++i;
							ClearAllNextUnusableCharacters(jsonText, ref i);
						}
						break;
				}
			}

			primitiveArrayNode.PrimitiveChildren = foundValueStrings.ToArray();
			return primitiveArrayNode;

			void ClearAllNextUnusableCharacters(string text, ref int index)
			{
				for (; index < text.Length; ++index)
				{
					var newChar = jsonText[index];
					switch (newChar)
					{
						case ',':
						case '\n':
						case '}':
						case ']':
							break;
						case '\t':
							lastInvalidCharIndex = index;
							break;
						default:
							--index;
							return;
					}
				}
			}
		}

		private static JsonObjectNode CreateObjectNodeFromJson(string jsonText, ref int i, bool typeSerialization, string label)
		{
			var startIndex = ++i;
			var endIndex = jsonText.Length - 1;
			var depth = 0;
			for (; i < jsonText.Length; ++i)
			{
				var newChar = jsonText[i];
				if (newChar.Equals('{'))
				{
					++depth;
					continue;
				}

				if (newChar.Equals('}'))
				{
					if (depth == 0)
					{
						endIndex = i;
						break;
					}

					--depth;
				}
			}

			var objectNode = new JsonObjectNode()
			{
				Name = label
			};
			endIndex -= startIndex;

			var subString = jsonText.Substring(startIndex, endIndex);
			var newNodes = CreateNodesFromData(subString, 0, typeSerialization);
			for (var index = 0; index < newNodes.Count; index++)
			{
				var newNode = newNodes[index];
				// Handle type differently.
				if (newNode.Name == STB_ASSEMBLY_TYPE_NAME && newNode is JsonSimpleNode typeNode)
				{
					if (index == 0)
					{
						objectNode.Type = (string)typeNode.Value;
					}
					else
					{
						// If it is a a type node but it is not the initial child node then it is the type for the previous node.
						var previousNode = newNodes[index - 1];
						previousNode.Type = (string)typeNode.Value;
					}
				}
				else
				{
					objectNode.AddChild(newNode);
				}
			}

			return objectNode;
		}

		private static bool TryCreateSimpleNodeFromJson(string jsonText, ref int i, string label, out JsonSimpleNode simpleNode)
		{
			// We add 1 to value start to ignore the colon.
			simpleNode = null;
			var valueStart = i;
			var insideString = false;

			for (; i < jsonText.Length; ++i)
			{
				var comparisonChar = jsonText[i];
				if (comparisonChar.Equals('"'))
				{
					insideString = !insideString;
				}

				if (insideString) continue;

				if (comparisonChar.Equals('}') || comparisonChar.Equals(']') || comparisonChar.Equals(',') || comparisonChar.Equals('\n') || i == jsonText.Length - 1)
				{
					var valueEnd = i;
					// Add another to the end if it's because we ran out of text as we account for the extra characters.
					if (!comparisonChar.Equals('\n') && i == jsonText.Length - 1)
					{
						valueEnd++;
					}

					var value = jsonText.Substring(valueStart, valueEnd - valueStart);
					// If it was a string remove the extra ". Also handle the value as a string.
					if (value[0].Equals('"') && value[value.Length - 1].Equals('"'))
					{
						simpleNode = new JsonSimpleNode(label, value.Trim('"'));
						return true;
					}

					simpleNode = new JsonSimpleNode(label, value);
					return true;
				}
			}

			return false;
		}

		private static bool TryGetNodeLabel(string targetString, ref int index, out string label)
		{
			label = string.Empty;
			var createdLabel = false;
			for (; index < targetString.Length; index++)
			{
				var tokenChar = targetString[index];
				switch (tokenChar)
				{
					case '{':
					case '[':
						return false;
					case ',': // If we hit a comma it indicates the node before in the array wasn't fully handled.
						break;
					case ':':
						if (createdLabel) return true;
						if (SaveToolboxPreferences.Instance.LoggingEnabled) Debug.LogWarning("Found : where there shouldn't have been.");
						return false;
					case '"':
					{
						var labelStartIndex = index;
						index = labelStartIndex;

						for (index += 1; index < targetString.Length; index++)
						{
							var comparisonChar = targetString[index];
							if (comparisonChar.Equals('"'))
							{
								var labelLength = index - labelStartIndex - 1;
								label = targetString.Substring(labelStartIndex + 1, labelLength);
								createdLabel = true;
								break;
							}
						}
						if (!createdLabel) 	throw new Exception("Found start of label but no end.");
						break;
					}
				}
			}

			return false;
		}

		internal static async Task<List<JsonBaseNode>> CreateNodesFromDataAsync(string jsonText, int stringIndex, bool typeSerialization = true, bool hasSingularLabel = false)
		{
			var nodeList = new List<JsonBaseNode>();
			var hasAttemptedToGetLabel = false;
			var label = string.Empty;

			for (var i = stringIndex; i < jsonText.Length; ++i)
			{
				// Try and get a label.
				if (!hasAttemptedToGetLabel)
				{
					hasAttemptedToGetLabel = true;
					var prevIndex = i;
					if (!TryGetNodeLabel(jsonText, ref i, out label))
					{
						i = prevIndex;
					}
					else
					{
						// If the whole thing is a label?
						if (i >= jsonText.Length) break;

						if (jsonText[i] == '"') i++;
					}
					await CheckFrameTime();
				}

				var tokenChar = jsonText[i];
				switch (tokenChar)
				{
					// Handle non-simple nodes without labels.
					case '{':
						if (!hasSingularLabel)
						{
							hasAttemptedToGetLabel = false;
						}

						var objectNode = CreateObjectNodeFromJson(jsonText, ref i, typeSerialization, label);
						objectNode.Name = label;
						nodeList.Add(objectNode);
						await CheckFrameTime();
						break;
					case '[':
						if (!hasSingularLabel)
						{
							hasAttemptedToGetLabel = false;
						}

						var arrayNode = CreateArrayNodeFromJson(jsonText, ref i, typeSerialization, label);
						arrayNode.Name = label;
						nodeList.Add(arrayNode);
						await CheckFrameTime();
						break;
					// Handle node values.
					case ':':
					case ',':
					case ' ':
					case '\t':
					case '\n':
						break;
					default:
					{
						if (!hasSingularLabel)
						{
							hasAttemptedToGetLabel = false;
						}

						if (TryCreateSimpleNodeFromJson(jsonText, ref i, label, out var simpleNode))
						{
							nodeList.Add(simpleNode);
							await CheckFrameTime();
						}

						break;
					}
				}
			}

			return nodeList;
		}

		private static async Task CheckFrameTime()
		{
			currentFrameTime = Time.realtimeSinceStartup;
			var difference = currentFrameTime - startFrameTime;
			if (difference > 1f / SaveToolboxPreferences.Instance.LowestAcceptableLoadingFrameRate)
			{
				await Task.Yield();
				startFrameTime = Time.realtimeSinceStartup;
			}
		}
	}
}