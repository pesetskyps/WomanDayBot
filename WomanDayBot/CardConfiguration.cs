using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WomanDayBot
{
    public class CardConfiguration
    {
        public CardConfiguration(string ImageUrl, string TitleText, string Description, string TitleId)
        {
            this.TitleId = TitleId;
            this.Description = Description;
            this.ImageUrl = ImageUrl;
            this.TitleText = TitleText;
        }

        public string ImageUrl { get; private set; }
        public string TitleText { get; private set; }
        public string Description { get; private set; }
        public string TitleId { get; private set; }
    }
}
