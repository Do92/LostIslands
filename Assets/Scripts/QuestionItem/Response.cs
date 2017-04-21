using System;
using Miscellaneous;

namespace QuestionItem
{
    [Serializable]
    public struct ClientResponseInfo
    {
        public QuestionType QuestionType;
        public AnswerData AnswerKeyData;
        public bool IsQuestionAnswered;

        public ClientResponseInfo(QuestionType questionType, AnswerData answerKeyData = AnswerData.None, bool isQuestionAnswered = false)
        {
            // Allow maximal 1 valid answer key for multiple choice, throws an exception on invalid data
            if (questionType == QuestionType.MultipleChoiceItem)
                answerKeyData.ValidateFlags(ExtensionMethods.AllowedFlagsData.MaxOne, true);

            QuestionType = questionType;
            AnswerKeyData = answerKeyData;
            IsQuestionAnswered = isQuestionAnswered;
        }
    }
}