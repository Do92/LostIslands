using System;
using System.Collections.Generic;
using Miscellaneous;

namespace QuestionItem
{
    public enum QuestionType
    {
        MultipleChoiceItem,
        MultipleResponseItem
    }

    [Serializable]
    public struct QuestionInfo
    {
        // Information regarding the actual question itself
        public QuestionType QuestionType;
        public string QuestionText;

        // All the given answer options with the answer key(s)
        public AnswerInfo[] AnswerOptions;
        public AnswerData AnswerKeyData;

        // Explanation of the question for the client result feedback panel
        public string Description;

        // NOTE: this might need to become dynamic instead of an object
        // Returns either a single answerKey string or a collection of answerKey strings
        public object AnswerKeyDataText
        {
            get
            {
                if (QuestionType == QuestionType.MultipleChoiceItem)
                    foreach (AnswerInfo answerOption in AnswerOptions)
                        if (answerOption.AnswerData.Equals(AnswerKeyData))
                            return answerOption.AnswerText;

                if (QuestionType == QuestionType.MultipleResponseItem)
                {
                    // NOTE: this might need to become a list instead of an array
                    //string[] answerKeyTextCollection = { };
                    List<string> answerKeyTextCollection = new List<string>();
                    //int index = 0;

                    foreach (AnswerInfo answerOption in AnswerOptions)
                        if (answerOption.AnswerData.Equals(AnswerKeyData))
                            //answerKeyTextCollection[index++] = answerOption.AnswerText;
                            answerKeyTextCollection.Add(answerOption.AnswerText);

                    return answerKeyTextCollection.ToArray();
                }

                // It is very unlikely that this will happen, but just in case nothing has been returned yet
                throw new Exception("No AnswerKeyText has been found, so this QuestionInfo contains invalid data!");
            }
        }

        public QuestionInfo(QuestionType questionType, string questionText, AnswerInfo[] answerOptions, AnswerData answerKeyData, string description)
        {
            // Allow only 1 valid answer key for multiple choice, throws an exception on invalid data
            if (questionType == QuestionType.MultipleChoiceItem)
                answerKeyData.ValidateFlags(ExtensionMethods.AllowedFlagsData.OnlyOne, true);
            // Allow minimal 1 valid answer key for multiple response, throws an exception on invalid data
            else if (questionType == QuestionType.MultipleResponseItem)
                answerKeyData.ValidateFlags(ExtensionMethods.AllowedFlagsData.MinOne, true);

            QuestionType = questionType;
            QuestionText = questionText;
            AnswerOptions = answerOptions;
            AnswerKeyData = answerKeyData;
            Description = description;
        }
    }
}