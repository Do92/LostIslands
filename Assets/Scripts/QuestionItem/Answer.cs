using System;
using Miscellaneous;

namespace QuestionItem
{
    [Flags]
    public enum AnswerData : byte
    {
        None = 0,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,
        D = 1 << 3
    }

    // General information of a certain answer
    [Serializable]
    public struct AnswerInfo
    {
        public AnswerData AnswerData; // Enumeration type of the answer
        public string AnswerText; // Read from xml data by the QuestionReader

        public AnswerInfo(AnswerData answerData, string answerText)
        {
            // Allow only 1 valid answer type, throws an exception on invalid data
            answerData.ValidateFlags(ExtensionMethods.AllowedFlagsData.OnlyOne, true);

            AnswerData = answerData;
            AnswerText = answerText;
        }
    }
}