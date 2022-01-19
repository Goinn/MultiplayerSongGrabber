using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using Newtonsoft.Json;
using MelonLoader;
using UnityEngine;
using UnityEngine.Networking;
using VRTK.UnityEventHelper;
using Synth.Utils;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.IO;

namespace MultiplayerSongGrabber
{
    public class BeatmapList
    {
        public BeatmapInfoZ[] Property1 { get; set; }
    }
    public class BeatmapInfoZ
    {
        public int id { get; set; }
        public string hash { get; set; }
        public string title { get; set; }
        public string artist { get; set; }
        public string mapper { get; set; }
        public string duration { get; set; }
        public string bpm { get; set; }
        public string[] difficulties { get; set; }
        public string description { get; set; }
        public string youtube_url { get; set; }
        public string filename { get; set; }
        public string filename_original { get; set; }
        public int cover_version { get; set; }
        public int play_count { get; set; }
        public int play_count_daily { get; set; }
        public int download_count { get; set; }
        public int upvote_count { get; set; }
        public int downvote_count { get; set; }
        public int vote_diff { get; set; }
        public string score { get; set; }
        public string rating { get; set; }
        public bool published { get; set; }
        public bool production_mode { get; set; }
        public bool beat_saber_convert { get; set; }
        public bool _explicit { get; set; }
        public bool ost { get; set; }
        public DateTime published_at { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int version { get; set; }
        public User user { get; set; }
        public object[] collaborators { get; set; }
        public string download_url { get; set; }
        public string cover_url { get; set; }
        public string preview_url { get; set; }
        public string video_url { get; set; }
    }

    public class User
    {
        public int id { get; set; }
        public string username { get; set; }
        public string avatar_filename { get; set; }
        public string avatar_url { get; set; }
    }

    public class Downloader : MelonMod
    {

        public static Downloader cs_instance;

        public void DownloadSong(object sender, VRTK.InteractableObjectEventArgs e)
        {
            //get current selected song
            //check if already downloaded -> enable button only if song is missing?
            //send request to synthriderz
            //save to CustomSongs
            //reload songs
            //disable button?
            //profit!
            Game_InfoProvider gipInstance = Game_InfoProvider.s_instance;

            Type ssm = typeof(Synth.SongSelection.SongSelectionManager);
            FieldInfo ssmInstanceInfo = ssm.GetField("s_instance", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            Synth.SongSelection.SongSelectionManager ssmInstance = (Synth.SongSelection.SongSelectionManager)ssmInstanceInfo.GetValue(null);

            if (GameObject.Find("NoSongFound") != null)
            {
                string title = Game_InfoProvider.LasPlayID;
                string hash = Game_InfoProvider.LastPlayHash;
                string author = ssmInstance.SelectedGameTrack.Author;
                string beatmapper = ssmInstance.SelectedGameTrack.Beatmapper;
                MelonLogger.Log(title);
                //MelonCoroutines.Start(GetSong(title, author, beatmapper));
                MelonCoroutines.Start(GetSongwithHash(hash));
            }
        }

        public IEnumerator GetSongwithHash(string hash)
        {
            Type ssm = typeof(Synth.SongSelection.SongSelectionManager);
            FieldInfo ssmInstanceInfo = ssm.GetField("s_instance", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            Synth.SongSelection.SongSelectionManager ssmInstance = (Synth.SongSelection.SongSelectionManager)ssmInstanceInfo.GetValue(null);
           
            //download song
            MelonLogger.Msg("Found beatmap");
            string download_url = "synthriderz.com" + "/api/beatmaps/hash/download/" + hash;
            MelonLogger.Msg(download_url);
            using (UnityWebRequest songRequest = UnityWebRequest.Get(download_url))
            {
                string customsPath = Application.dataPath + "/../CustomSongs/";
                songRequest.downloadHandler = new DownloadHandlerFile(customsPath + "dump.synth");
                yield return songRequest.SendWebRequest();
                if (songRequest.isNetworkError)
                {
                    MelonLogger.Msg("GetSong error");
                }
                else
                {
                    MelonLogger.Msg("Download successful");
                    //rename file
                    if (File.Exists(customsPath + "dump.synth"))
                    {
                        string fileName = songRequest.GetResponseHeader("content-disposition").Split('"')[1];
                        MelonLogger.Msg(fileName);
                        File.Move(customsPath + "dump.synth", customsPath + fileName);
                    }
                    ssmInstance.RefreshSongList(false);
                    MelonLogger.Msg("Updated song list");
                }
            }
        }

        public IEnumerator GetSong(string title, string artist, string beatmapper)
        {
            Type ssm = typeof(Synth.SongSelection.SongSelectionManager);
            FieldInfo ssmInstanceInfo = ssm.GetField("s_instance", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            Synth.SongSelection.SongSelectionManager ssmInstance = (Synth.SongSelection.SongSelectionManager)ssmInstanceInfo.GetValue(null);
            artist.Replace("&", "%26");
            string requestUrl = $"synthriderz.com/api/beatmaps?s={{\"title\": \"{title}\", \"artist\": \"{artist}\", \"mapper\": \"{beatmapper}\"}}";
            using (UnityWebRequest webRequest = UnityWebRequest.Get(requestUrl))
            {
                yield return webRequest.SendWebRequest();
                //convert to tjson?
                //get hash
                //download map -> synthriderz.com/api/veatnaos/hash/download/ + hash
                if (webRequest.isNetworkError)
                {
                    MelonLogger.Msg("GetSong error");
                }
                else
                {
                    BeatmapInfoZ[] beatmapInfo = JsonConvert.DeserializeObject<BeatmapInfoZ[]>(webRequest.downloadHandler.text);
                    

                    if (beatmapInfo.Length == 1)
                    {
                        //download song
                        MelonLogger.Msg("Found beatmap");
                        string download_url = "synthriderz.com" + beatmapInfo[0].download_url;
                        MelonLogger.Msg(download_url);
                        using (UnityWebRequest songRequest = UnityWebRequest.Get(download_url))
                        {
                            string customsPath = Application.dataPath + "/../CustomSongs/" + beatmapInfo[0].filename;
                            songRequest.downloadHandler = new DownloadHandlerFile(customsPath);
                            yield return songRequest.SendWebRequest();
                            if (webRequest.isNetworkError)
                            {
                                MelonLogger.Msg("GetSong error");
                            }
                            else
                            {
                                MelonLogger.Msg("Download successful");
                                ssmInstance.RefreshSongList(false);
                                MelonLogger.Msg("Updated song list");
                            }
                        }
                    }
                    else
                    {
                        MelonLogger.Msg("Found more than one or no beatmaps");
                    }
                }
            }
        }

        //place button in multi lobby menu panel
        private static void ButtonInit()
        {
            var cs_instance = new Downloader();
            MelonLogger.Msg("MultiDownloader init");

            //Initialise new button
            GameObject multiplayer = GameObject.Find("Multiplayer");
            Transform roomPanel = multiplayer.transform.Find("RoomPanel");
            Transform rooms = roomPanel.Find("Rooms");
            Transform leftPanel = rooms.Find("LeftPanel");
            Transform infoWrap = leftPanel.Find("InfoWrap");
            Transform disconnectButton = infoWrap.Find("StandardButton - Disconnect");
            GameObject downloadButton = GameObject.Instantiate(disconnectButton.gameObject);
            downloadButton.transform.name = "DownloadSongButton";
            downloadButton.transform.SetParent(infoWrap);

            //set button position
            downloadButton.transform.localScale = new Vector3(1, 0.85f, 1);
            downloadButton.transform.localPosition = new Vector3(11.2f, 4.28f, 0);
            downloadButton.transform.localRotation = new Quaternion(0, 0, 0, 1);

            //set button label
            Transform buttonLabel = downloadButton.transform.Find("Label");
            buttonLabel.GetComponentInChildren<LocalizationHelper>().enabled = false;
            buttonLabel.GetComponentInChildren<TMPro.TMP_Text>().text = "Get song";

            Type eventHelper = typeof(VRTK_InteractableObject_UnityEvents);
            VRTK_InteractableObject_UnityEvents buttonEvent = (VRTK_InteractableObject_UnityEvents)downloadButton.GetComponent(eventHelper);
            MelonLogger.Msg("test");
            buttonEvent.OnUse.RemoveAllListeners();
            buttonEvent.OnUse.SetPersistentListenerState(1, UnityEngine.Events.UnityEventCallState.Off);
            buttonEvent.OnUse.AddListener(cs_instance.DownloadSong);
            downloadButton.SetActive(true);
            MelonLogger.Msg("Button added");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            var mainMenuScenes = new List<string>()
            {
                "01.The Room",
                "02.The Void",
                "03.Roof Top",
                "04.The Planet",
                "SongSelection"
            };
            base.OnSceneWasInitialized(buildIndex, sceneName);
            if (mainMenuScenes.Contains(sceneName)) ButtonInit();
        }

    }
}
