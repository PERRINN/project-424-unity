using UnityEngine;
using System;
using System.Collections;

namespace UniCAVE
{
	/// <summary>
	/// Property attribute to enable/disable
	/// fields in the inspector by boolean value.
	/// 
	/// Author:
	/// http://www.brechtos.com/hiding-or-disabling-inspector-properties-using-propertydrawers-within-unity-5/
	/// 
	/// 
	/// MODIFICATIONS:
	/// Author: Christoffer A Træen
	/// Added reversing so we can enable a property and also disable another 
	/// with the same controling source.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
		AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
	public class ConditionalHideAttribute : PropertyAttribute
	{
		/// <summary>
		/// The name of the field we are controlling
		/// </summary>
		public string ConditionalSourceField = "";

		/// <summary>
		/// If true, hides the attributes in the inspector,
		/// if false, fields are visible but disabled (greyed out)
		/// </summary>
		public bool HideInInspector = false;

		/// <summary>
		/// If enabled, the attribute is enabled when the controling flag is false.
		/// </summary>
		public bool Reverse = false;

		/// <summary>
		/// Sets the name of the controlling attribute
		/// set <c>HideInInspector</c> and <c>Reverse</c> to <c>FALSE</c>
		/// </summary>
		/// <param name="conditionalSourceField">The controling attribute</param>
		public ConditionalHideAttribute(string conditionalSourceField)
		{
			this.ConditionalSourceField = conditionalSourceField;
			this.HideInInspector = false;
			this.Reverse = false;
		}

		/// <summary>
		/// Sets the name of the controlling attribute
		/// sets <c>Reverse</c> to provided value
		/// set <c>HideInInspector</c> to <c>FALSE</c>
		/// </summary>
		/// <param name="conditionalSourceField">The controling attribute</param>
		/// <param name="reverse">The reverse flag</param>
		public ConditionalHideAttribute(string conditionalSourceField, bool reverse)
		{
			this.ConditionalSourceField = conditionalSourceField;
			this.HideInInspector = false;
			this.Reverse = reverse;
		}

		/// <summary>
		/// Sets the name of the controlling attribute
		/// sets <c>Reverse</c> to provided value
		/// set <c>HideInInspector</c> to provided vaue
		/// </summary>
		/// <param name="conditionalSourceField">The controling attribute</param>
		/// <param name="reverse">The reverse flag</param>
		/// <param name="hideInInspector">The hideInInspector flag</param>
		public ConditionalHideAttribute(string conditionalSourceField, bool reverse, bool hideInInspector)
		{
			this.ConditionalSourceField = conditionalSourceField;
			this.Reverse = reverse;
			this.HideInInspector = hideInInspector;
		}

	}
}