﻿
namespace KSoft.Phoenix.Phx
{
	public sealed class BTrigger
		: TriggerScriptIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Trigger")
		{
			DataName = DatabaseNamedObject.kXmlAttrNameN,
		};

		const string kXmlAttrActive = "Active";
		const string kXmlAttrEvaluateFrequency = "EvaluateFrequency";
		const string kXmlAttrEvalLimit = "EvalLimit";
		const string kXmlAttrConditionalTrigger = "ConditionalTrigger";
	#endregion

	bool mActive;

		int mEvaluateFrequency;

		int mEvalLimit;

		bool mConditionalTrigger;

		public Collections.BListAutoId<BTriggerCondition> Conditions { get; private set; } = new Collections.BListAutoId<BTriggerCondition>();

		/// <summary>True if <see cref="Conditions"/> are OR, false if they're AND</summary>
		public bool OrConditions { get; set; }
		public Collections.BListAutoId<BTriggerEffect> EffectsOnTrue { get; private set; } = new Collections.BListAutoId<BTriggerEffect>();
		public Collections.BListAutoId<BTriggerEffect> EffectsOnFalse { get; private set; } = new Collections.BListAutoId<BTriggerEffect>();

		void StreamConditions<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var k_AND_params = BTriggerCondition.kBListXmlParams_And;

			if (s.IsReading)
			{
				if (this.OrConditions = !s.ElementsExists(k_AND_params.RootName))
					XML.XmlUtil.Serialize(s, this.Conditions, BTriggerCondition.kBListXmlParams_Or);
				else
					XML.XmlUtil.Serialize(s, this.Conditions, k_AND_params);
			}
			else if (s.IsWriting)
			{
				// Even if there are no conditions, the runtime expects there to be an empty And tag :|
				// Well, technically we could use an empty Or tag as well, but it wouldn't be consistent
				// with the engine. The runtime will assume the the TS is bad if neither tag is present
				if (this.Conditions.Count == 0)
					s.WriteElement(k_AND_params.RootName);
				else
					XML.XmlUtil.Serialize(s,
					                      this.Conditions,
					                      this.OrConditions ? BTriggerCondition.kBListXmlParams_Or : k_AND_params);
			}
		}
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttribute(kXmlAttrActive, ref this.mActive);
			s.StreamAttribute(kXmlAttrEvaluateFrequency, ref this.mEvaluateFrequency);
			s.StreamAttribute(kXmlAttrEvalLimit, ref this.mEvalLimit);
			s.StreamAttribute(kXmlAttrConditionalTrigger, ref this.mConditionalTrigger);

			// These tags must exist no matter what :|
			using (s.EnterCursorBookmark(BTriggerCondition.kXmlRootName))
				this.StreamConditions(s);

			using (s.EnterCursorBookmark(BTriggerEffect.kXmlRootName_OnTrue))
				XML.XmlUtil.Serialize(s, this.EffectsOnTrue, BTriggerEffect.kBListXmlParams);

			using (s.EnterCursorBookmark(BTriggerEffect.kXmlRootName_OnFalse))
				XML.XmlUtil.Serialize(s, this.EffectsOnFalse, BTriggerEffect.kBListXmlParams);
		}
	};
}
