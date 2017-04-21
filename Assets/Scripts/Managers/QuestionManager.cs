using UnityEngine;
using System;
using System.Collections.Generic;
using Miscellaneous;
using System.Xml;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using QuestionItem;

namespace Managers
{
    /// <summary>
    /// This class is used for managing everything around the questions.
    /// </summary>
    public class QuestionManager : Singleton<QuestionManager>
    {
        [HideInInspector]
        public List<TextAsset> XmlQuestionLists = new List<TextAsset>();

        public List<QuestionInfo> QuestionList = new List<QuestionInfo>();
        //private Dictionary<QuestionType, List<Question>> questionCollection = new Dictionary<QuestionType, List<Question>>();

        // Initialization.
        private void Awake()
        {
            LoadXmlQuestionLists();
        }

        // Loads the xml question lists out of the Resources/XmlQuestionLists folder.
        private void LoadXmlQuestionLists()
        {
            foreach (TextAsset xmlFile in Resources.LoadAll<TextAsset>("XmlQuestionLists"))
                XmlQuestionLists.Add(xmlFile);
        }

        /// <summary>
        /// This method reads the specified xml question list and adds the question items to this.questionList
        /// Note: debugging  by reading in XML causes it to continue.
        /// </summary>
        /*private void ReadXmlQuestionList(TextAsset xmlQuestionList)
        {
            ClearQuestionList();
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlQuestionList.text)))
            {
                // Read(add) all question items
                while (reader.Read())
                {
                    //print("count");

                    //Reads the question type
                    reader.MoveToNextAttribute();
                    //Debug.Log("Type: "+ reader.Value);
                    //print("next question :" + reader.Value);

                    // Read(add) the next question item of a particular QuestionType
                    switch (reader.Value)
                    {
                        case "multiplechoiceAnswer":
                            QuestionType questionType = QuestionType.MultipleChoiceItem;
                            reader.Read();
                            //Reads to the question.
                            reader.ReadToFollowing("question_text");
                            //print("Question string: " + reader.ReadString());
                            string questionText = reader.ReadString();
                            //print("Question string: " + reader.ReadString());

                            int answerCount = Enum.GetNames(typeof(AnswerData)).Length - 1; // 1 is to exclude AnswerData.None

                            AnswerInfo[] answerOptions = new AnswerInfo[answerCount];

                            //reader.ReadToFollowing("option_a");
                            for (int i = 0; i < answerCount; i++)
                            {
                                if (reader.NodeType == XmlNodeType.EndElement)
                                {
                                    //print("read end element");
                                    reader.ReadEndElement();
                                }

                                //print(i);
                                reader.ReadStartElement();
                                AnswerInfo answerOption = new AnswerInfo((AnswerData)(1 << i), reader.ReadString());

                                answerOptions[i] = answerOption;

                            }
                            reader.ReadToFollowing("correct_option");

                            AnswerData answerKeyData = (AnswerData)Enum.Parse(typeof(AnswerData), reader.ReadString(), true);

                            questionList.Add(new QuestionInfo(questionType, questionText, answerOptions, answerKeyData));
                            //AddQuestion(new QuestionInfo(questionType, questionText, answerOptions, answerKeyData));
                            break;
                    }
                }
            }
        }*/

        /// <summary>
        /// Changes the current question list to a new question list,
        /// which is constructed with the xml question list content.
        /// 
        /// This uses the existing collection "XmlQuestionLists" with the specified index to get the desired xml question list.
        /// That xml question list will then be used as argument to the overloaded method.
        /// 
        /// NOTE: This is mainly intended for usage within the editor
        /// </summary>
        /// <param name="desiredListIndex">The desired list index of the xml question list collection.</param>
        public void ChangeQuestionList(int desiredListIndex)
        {
            // It basically empties currentQuestionList and adds the desired question list items to it
            ChangeQuestionList(XmlQuestionLists[desiredListIndex]);
        }

        /// <summary>
        /// Changes the current question list to a new question list,
        /// which is constructed with the specified xml question list content.
        /// </summary>
        /// <param name="xmlQuestionList">The xml question list with content for the new question list.</param>
        private void ChangeQuestionList(TextAsset xmlQuestionList)
        {
            ClearQuestionList();

            using (XmlReader reader = XmlReader.Create(new StringReader(xmlQuestionList.text)))
            {
                // Read(add) all question items
                while (reader.Read())
                {
                    // Reads the question type
                    reader.MoveToNextAttribute();

                    // Read(add) the next question item of a particular QuestionType
                    switch (reader.Value)
                    {
                        case "multipleChoiceItem":
                            {
                                QuestionType questionType = QuestionType.MultipleChoiceItem;
                                reader.Read();
                                // Reads to the question.
                                reader.ReadToFollowing("QuestionText");
                                string questionText = reader.ReadString();

                                int answerCount = Enum.GetNames(typeof(AnswerData)).Length - 1; // 1 is to exclude AnswerData.None
                                AnswerInfo[] answerOptions = new AnswerInfo[answerCount];

                                for (int i = 0; i < answerCount; i++)
                                {
                                    if (reader.NodeType == XmlNodeType.EndElement)
                                        reader.ReadEndElement();

                                    reader.ReadStartElement();
                                    AnswerInfo answerOption = new AnswerInfo((AnswerData)(1 << i), reader.ReadString());

                                    answerOptions[i] = answerOption;

                                }
                                reader.ReadToFollowing("CorrectOption");

                                AnswerData answerKeyData = (AnswerData)Enum.Parse(typeof(AnswerData), reader.ReadString(), true);

                                reader.ReadToFollowing("Description");

                                string description = reader.ReadString();

                                QuestionList.Add(new QuestionInfo(questionType, questionText, answerOptions, answerKeyData, description));
                                break;
                            }
                        case "multipleResponseItem":
                            {
                                QuestionType questionType = QuestionType.MultipleResponseItem;
                                reader.Read();
                                // Reads to the question.
                                reader.ReadToFollowing("QuestionText");
                                string questionText = reader.ReadString();

                                int answerCount = Enum.GetNames(typeof(AnswerData)).Length - 1; // 1 is to exclude AnswerData.None
                                AnswerInfo[] answerOptions = new AnswerInfo[answerCount];
                                AnswerData answerKeyData = AnswerData.None; // We will loop-over and set all the flags for this below

                                //int debugCounter = 0;

                                for (int i = 0; i < answerCount; i++)
                                {
                                    if (reader.NodeType == XmlNodeType.EndElement)
                                    {
                                        reader.ReadEndElement();
                                        reader.Skip();
                                    }

                                    reader.MoveToNextAttribute();

                                    string answerString = string.Empty;

                                    switch (reader.NodeType)
                                    {
                                        case XmlNodeType.None: Debug.Log(1); break;
                                        case XmlNodeType.Element: Debug.Log(2); break;
                                        case XmlNodeType.Attribute:
                                            //Debug.Log(debugCounter + "d - iteration: " + i);
                                            if (reader.Value == "correct")
                                            {
                                                //Debug.Log(debugCounter++ + "d - 3 - correct - " + reader.ReadString());
                                                answerString = reader.ReadString(); // setting the correct option
                                                //Debug.Log("i" + i + " - 3 - correct - " + correctOption);
                                                AnswerData answerData = (AnswerData)(1 << i);
                                                //Debug.Log(debugCounter++ + "d - " + i + "i - correct option: " + answerData + " - " + answerString);
                                                //Debug.Log(debugCounter++ + "d - " + i + "i - 3 - correct option: " + answerData + " - " + correctOption);

                                                answerKeyData |= (AnswerData)(1 << i);
                                                //Debug.Log(debugCounter++ + "d - 3 - correct - " + correctOption);
                                                //answerKeyData |= (AnswerData)Enum.Parse(typeof(AnswerData), correctOption, true);
                                                //answerKeyData |= (AnswerData)Enum.Parse(typeof(AnswerData), reader.ReadString(), true);
                                                //else if (reader.Value == "incorrect")
                                                //Debug.Log(debugCounter++ + "d - 3 - incorrect - " + reader.ReadString()); // NOTE: It might need to read this anyway (even though I don't need it)
                                            }
                                            else if (reader.Value == "incorrect")
                                                answerString = reader.ReadString();
                                            break;
                                        case XmlNodeType.Text: Debug.Log(4); break;
                                        case XmlNodeType.CDATA: Debug.Log(5); break;
                                        case XmlNodeType.EntityReference: Debug.Log(6); break;
                                        case XmlNodeType.Entity: Debug.Log(7); break;
                                        case XmlNodeType.ProcessingInstruction: Debug.Log(8); break;
                                        case XmlNodeType.Comment: Debug.Log(9); break;
                                        case XmlNodeType.Document: Debug.Log(10); break;
                                        case XmlNodeType.DocumentType: Debug.Log(11); break;
                                        case XmlNodeType.DocumentFragment: Debug.Log(12); break;
                                        case XmlNodeType.Notation: Debug.Log(13); break;
                                        case XmlNodeType.Whitespace: Debug.Log(14); break;
                                        case XmlNodeType.SignificantWhitespace: Debug.Log(15); break;
                                        case XmlNodeType.EndElement: Debug.Log(16); break;
                                        case XmlNodeType.EndEntity: Debug.Log(17); break;
                                        case XmlNodeType.XmlDeclaration: Debug.Log(18); break;
                                        default: throw new ArgumentOutOfRangeException();
                                    }


                                    //answerKeyData |= (AnswerData)Enum.Parse(typeof(AnswerData), readedString, true);

                                    // Creates the answer option (of the current iteration e.g., AnswerData.A or AnswerData.B)
                                    //answerString = reader.ReadString();
                                    if (answerString == string.Empty)
                                    {
                                        Debug.Log("empty string");
                                        continue;
                                    }

                                    //Debug.Log(debugCounter++ + "d + available option: " + answerString);
                                    AnswerInfo answerOption = new AnswerInfo((AnswerData)(1 << i), answerString);

                                    // If there is a question type, then we save this as a correct option
                                    //if (reader.ReadAttributeValue())
                                    //{
                                    //    Debug.Log("This never executes");
                                    //    answerKeyData |= (AnswerData)Enum.Parse(typeof(AnswerData), reader.ReadString(), true);
                                    //}

                                    //Debug.Log(debugCounter++ + "d - Iteration " + i);
                                    answerOptions[i] = answerOption;
                                }

                                if (answerKeyData == AnswerData.None)
                                {
                                    Debug.Log("answerKeyData = empty");
                                    continue;
                                }

                                //Debug.Log(answerKeyData);

                                //Debug.Log("Count: " + answerOptions.Count());
                                //Debug.Log("Length: " + answerOptions.Length);

                                //foreach (AnswerInfo answerOption in answerOptions)
                                //Debug.Log("AnswerData: " + answerOption.AnswerData + " - AnswerText: " + answerOption.AnswerText);

                                reader.ReadToFollowing("Description");

                                string description = reader.ReadString();

                                QuestionList.Add(new QuestionInfo(questionType, questionText, answerOptions, answerKeyData, description));
                                break;
                            }
                    }
                }
            }
        }

        // Loads the questions out of the questions array.
        /*public void SelectQuestionList(int specifiedList)
        {
            // Clears this.questionList(List<QuestionInfo>) and adds the specified question list items(QuestionInfo) to it
            ReadXmlQuestionList(XmlQuestionLists[specifiedList]);
        }*/

        // Adds a question to the question list. //Might be unnecessary/unused.. not sure
        /*public void AddQuestion(QuestionInfo question)
        {
            questionList.Add(question);

            //switch (question.questionType) {
            //    case QuestionType.MultipleChoice:
            //        multipleAnswerQuestions.Add(question);
            //        break;
            //}
        }*/

        // Returns a completely random question.
        public QuestionInfo GetRandomQuestion()
        {
            int randomIndex = UnityEngine.Random.Range(0, QuestionList.Count);
            return QuestionList[randomIndex];
        }

        // Returns the next question in the list.
        public QuestionInfo GetNextQuestion(int roundNumber)
        {
            // The modulo operation excludes the previously-given questions amount of complete sets only
            // Then it has a remaining questions amount which is a valid index to get the next question
            return QuestionList[roundNumber % QuestionList.Count];
        }

        // Shuffles the question list.
        public void ShuffleQuestionList()
        {
            QuestionList.Shuffle();
        }

        // Clears the question list.
        public void ClearQuestionList()
        {
            QuestionList.Clear();
        }

        // Checks whether the answer key data from the question and the response are equal
        public static bool ValidateAnswerKeyData(QuestionInfo question, ClientResponseInfo response, bool isServer)
        {
            // TODO: Use method content below instead of the whole unnecessary method, if we don't need the commented switch
            return question.AnswerKeyData.Equals(response.AnswerKeyData);

            //switch (question.QuestionType)
            //{
            //    //case QuestionType.MultipleResponseItem: // This might actually work
            //    case QuestionType.MultipleChoiceItem:
            //        return question.AnswerKeyData.Equals(response.AnswerKeyData);
            //    default:
            //        Debug.LogWarning("Answer couldn't be checked. Points are awarded");
            //        return true;
            //}
        }
    }
}