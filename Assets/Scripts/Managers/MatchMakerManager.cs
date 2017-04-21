using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections.Generic;
using Miscellaneous;

namespace Managers
{
    // UNUSED
    public class MatchMakerManager : Singleton<MatchMakerManager>
    {
        public List<MatchDesc> MatchList = new List<MatchDesc>();
        private bool isMatchCreated;
        private NetworkMatch networkMatch;

        public delegate void MatchListEvent();

        public event MatchListEvent OnMatchList = delegate { };

        public delegate void MatchCreateEvent();

        public event MatchCreateEvent OnMatchCreate = delegate { };

        public delegate void MatchJoinedEvent();

        public event MatchJoinedEvent OnMatchJoined = delegate { };

        private void Awake()
        {
            networkMatch = gameObject.AddComponent<NetworkMatch>();
        }

        public void CreateMatch(string name, uint slots)
        {
            CreateMatchRequest match = new CreateMatchRequest();
            match.name = name;
            match.size = slots;
            match.advertise = true;
            match.password = "";

            networkMatch.CreateMatch(match, MatchCreated);
        }

        public void Refresh()
        {
            networkMatch.ListMatches(0, 20, "", SetMatchList);
        }

        public void JoinMatch(MatchDesc match)
        {
            networkMatch.JoinMatch(match.networkId, "", MatchJoined);
        }

        public void SetMatchList(ListMatchResponse matchListResponse)
        {
            if (matchListResponse.success && matchListResponse.matches != null)
            {
                MatchList = matchListResponse.matches;
                OnMatchList();
            }
        }

        private void MatchCreated(CreateMatchResponse matchResponse)
        {
            if (matchResponse.success)
            {
                Debug.Log("Create match succeeded");
                isMatchCreated = true;
                Utility.SetAccessTokenForNetwork(matchResponse.networkId, new NetworkAccessToken(matchResponse.accessTokenString));
                NetworkServer.Listen(new MatchInfo(matchResponse), 9000);
                OnMatchCreate();
            }
            else
                Debug.LogError("Create match failed");
        }

        private void MatchJoined(JoinMatchResponse matchJoin)
        {
            if (matchJoin.success)
            {
                Debug.Log("Join match succeeded");

                if (isMatchCreated)
                {
                    Debug.LogWarning("Match already set up, aborting...");
                    return;
                }

                Utility.SetAccessTokenForNetwork(matchJoin.networkId, new NetworkAccessToken(matchJoin.accessTokenString));
                NetworkClient client = new NetworkClient();

                client.RegisterHandler(MsgType.Connect, OnConnected);
                client.Connect(new MatchInfo(matchJoin));

                OnMatchJoined();
            }
            else
                Debug.LogError("Join match failed");
        }

        public void OnConnected(NetworkMessage msg)
        {
            Debug.Log("Connected!");
        }
    }
}