
namespace Discitur.QueryStack
{
    public class Constants
    {
        public const int RECORD_STATE_ACTIVE = 0;
        public const int RECORD_STATE_DELETED = 2;
        public const int LESSON_PUBLISHED = 1;
        public const int LESSON_NOT_PUBLISHED = 0;
        public const string LESSON_SEARCH_ORDER_FIELD = "PublishDate";
        public const string LESSON_SEARCH_ORDER_DIR = "DESC";

        public const string DISCITUR_ERRORS = "discerrors";
        public const string DISCITUR_ERROR_UNHANDLED = "discerr00";
        public const string DISCITUR_ERROR_USERNAME_USED = "discerr01";
        public const string DISCITUR_ERROR_EMAIL_USED = "discerr02";

        public const string USER_DEFAULT_PICTURE = "img/def-user-icon.png";
        public const int USER_PICTURE_MAXWIDTH = 300;
        public const int USER_PICTURE_MAXHEIGHT = 300;
        public const string USER_PICTURE_FORMAT = "jpg";
        public const int USER_THUMB_MAXWIDTH = 50;
        public const int USER_THUMB_MAXHEIGHT = 50;
        public const string USER_THUMB_FORMAT = "jpg";

    }
}