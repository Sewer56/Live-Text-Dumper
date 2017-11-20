using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualizerCurrentTime
{
    /// <summary>
    /// Provides various methods for the exporting and retrieval of various pieces of information which are to be printed to the screen or reused.
    /// </summary>
    class MusicBee
    {
        /// <summary>
        /// Stores the state of the active MusicBee process from which the currently playing track may be read in from.
        /// </summary>
        public Process[] musicBeeProcess;

        /// <summary>
        /// Specifies the current, currently shown and dumped song title.
        /// </summary>
        public string songTitle;

        /// <summary>
        /// The amount of characters to trim from the window title of MusicBee to obtain only the name of the played song.
        /// </summary>
        const int MUSICBEE_POST_SONG_TITLE_LENGTH = 11;

        /// <summary>
        /// Retrieves the currently playing song title from the MusicBee Process
        /// </summary>
        public void GetSongTitle()
        {
            // Retrieves the MusicBee Process -  Checks whether MusicBee is currently running.
            musicBeeProcess = Process.GetProcessesByName("MusicBee");
            
            // If at least one song process has been detected, grab the title of the first matched window.
            if (musicBeeProcess.Length != 0) { songTitle = musicBeeProcess[0].MainWindowTitle.Substring(0, musicBeeProcess[0].MainWindowTitle.Length - MUSICBEE_POST_SONG_TITLE_LENGTH); }
        }
    }
}
