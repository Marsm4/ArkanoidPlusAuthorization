namespace WebApplication2
{
    public class Skin
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string Photo { get; set; } // Ссылка на изображение

        // Навигационное свойство для связи с UserSkins
        public ICollection<UserSkin> UserSkins { get; set; }
    }
}