namespace cured
{
    public static class user
    {
        public static int id_user { get; set; } // Обязательно добавь это!
        public static string login_user { get; set; }
        public static string full_name { get; set; }
        public static string phone { get; set; }
        public static string role { get; set; }
    }

    public static class acc_checked
    {
        public static bool acc_check { get; set; } = false;
    }
}