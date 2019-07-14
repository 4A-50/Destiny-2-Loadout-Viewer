using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json.Linq;

namespace DestinyAPITest
{
    public partial class Form1 : Form
    {
        WebClient webClient = new WebClient();

        int membershipType = 0;
        long membershipId = 0;

        List<long> characters = new List<long>();

        JObject objectsItem = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            webClient.Headers.Add("X-API-Key", "");

            activateCharBtns();

            string urlItem = "https://destiny.plumbing/en/raw/DestinyInventoryItemDefinition.json";
            var jsonItem = webClient.DownloadString(urlItem);
            objectsItem = JObject.Parse(jsonItem);
        }
        void activateCharBtns()
        {
            if (characters.Count == 0)
            {
                CharBTN1.Visible = false;
                CharBTN2.Visible = false;
                CharBTN3.Visible = false;
            }
            if (characters.Count == 1)
            {
                CharBTN1.Visible = true;
                CharBTN2.Visible = false;
                CharBTN3.Visible = false;
            }
            if (characters.Count == 2)
            {
                CharBTN1.Visible = true;
                CharBTN2.Visible = true;
                CharBTN3.Visible = false;
            }
            if (characters.Count == 3)
            {
                CharBTN1.Visible = true;
                CharBTN2.Visible = true;
                CharBTN3.Visible = true;
            }

        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && InputBox.TextLength > 0)
            {
                searchUsers(InputBox.Text);
            }
        }

        void searchUsers(string username)
        {
            string prepedUser = username.Replace("#", "%23");
            string url = "https://www.bungie.net/Platform/Destiny2/SearchDestinyPlayer/-1/" + prepedUser + "/";

            var json = webClient.DownloadString(url);
            var objects = JObject.Parse(json);

            if (objects.SelectToken("Response").HasValues == true)
            {
                membershipType = (int)objects.SelectToken("Response[0].membershipType");
                membershipId = (long)objects.SelectToken("Response[0].membershipId");


                string url2 = "https://www.bungie.net/Platform/Destiny2/" + membershipType + "/Profile/" + membershipId + "/?components=100";

                var json2 = webClient.DownloadString(url2);
                var objects2 = JObject.Parse(json2);

                if (objects2.SelectToken("Response").HasValues == true)
                {
                    characters.Clear();
                    IEnumerable<JToken> list = objects2.SelectTokens("Response.profile.data.characterIds[*]");
                    foreach (var item in list)
                    {
                        characters.Add(Convert.ToInt64(item));
                    }
                    activateCharBtns();

                    int loopCount = 0;
                    foreach (var item in characters)
                    {
                        string url3 = "https://www.bungie.net/Platform/Destiny2/" + membershipType + "/Profile/" + membershipId + "/Character/" + item + "/?components=200";
                        var json3 = webClient.DownloadString(url3);
                        var objects3 = JObject.Parse(json3);

                        if (objects3.SelectToken("Response").HasValues == true)
                        {
                            string info = classLookUp((int)objects3.SelectToken("Response.character.data.classType")) + " : " + ((int)objects3.SelectToken("Response.character.data.light")).ToString();
                            charsBTN(info, loopCount);
                        }
                        else
                        {
                            errorShow();
                        }
                        loopCount++;
                    }
                }
                else
                {
                    errorShow();
                }
            }
            else
            {
                errorShow();
            }
        }

        string classLookUp(int val)
        {
            switch (val)
            {
                case 0:
                    return "Titan";
                    break;
                case 1:
                    return "Hunter";
                    break;
                case 2:
                    return "Warlock";
                    break;
            }
            return null;
        }

        void charsBTN(string info, int val)
        {
            switch (val)
            {
                case 0:
                    CharBTN1.Text = info;
                    break;
                case 1:
                    CharBTN2.Text = info;
                    break;
                case 2:
                    CharBTN3.Text = info;
                    break;
            }
        }

        void errorShow()
        {

        }

        private void CharBTN1_Click(object sender, EventArgs e)
        {
            showCharLoadout(0);
        }

        private void CharBTN2_Click(object sender, EventArgs e)
        {
            showCharLoadout(1);
        }

        private void CharBTN3_Click(object sender, EventArgs e)
        {
            showCharLoadout(2);
        }

        void showCharLoadout(int val)
        {
            string url = "https://www.bungie.net/Platform/Destiny2/" + membershipType + "/Profile/" + membershipId + "/Character/" + characters[val] + "/?components=205";

            var json = webClient.DownloadString(url);
            var objects = JObject.Parse(json);

            if (objects.SelectToken("Response").HasValues == true)
            {
                for (int i = 0; i < objects.SelectToken("Response.equipment.data.items").Count(); i++)
                {
                    if (i != 14 || i != 12)
                    {
                        uint itemId = (uint)objects.SelectToken("Response.equipment.data.items[" + i + "].itemHash");

                        searchItemDef(itemId, i);
                    }
                }
            }
            else
            {
                errorShow();
            }
        }

        void searchItemDef(uint itemID, int loopCount)
        {
            Task<string> itemTask = Task.Run(() => getItemDef(itemID));

            itemImgSetter(loopCount, itemTask.Result);
        }

        Task<string> getItemDef(uint itemID)
        {
            return Task.FromResult("https://www.bungie.net" + (string)objectsItem.SelectToken(itemID + ".displayProperties.icon"));
        }

        void itemImgSetter(int val, string imgPath)
        {
            switch (val)
            {
                case 0:
                    Gun1.Load(imgPath);
                    break;
                case 1:
                    Gun2.Load(imgPath);
                    break;
                case 2:
                    Gun3.Load(imgPath);
                    break;
                case 3:
                    Head.Load(imgPath);
                    break;
                case 4:
                    Arms.Load(imgPath);
                    break;
                case 5:
                    Chest.Load(imgPath);
                    break;
                case 6:
                    Legs.Load(imgPath);
                    break;
                case 7:
                    ClassArmour.Load(imgPath);
                    break;
                case 8:
                    Ghost.Load(imgPath);
                    break;
                case 9:
                    Sparrow.Load(imgPath);
                    break;
                case 10:
                    Ship.Load(imgPath);
                    break;
                case 11:
                    SubClass.Load(imgPath);
                    break;
                case 13:
                    Embelm.Load(imgPath);
                    break;
            }
        }
    }
}
