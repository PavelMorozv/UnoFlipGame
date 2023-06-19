using GameCore.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Action = GameCore.Enums.Action;

namespace ServerLib.GameContent
{
    public class AppDBContext : DbContext
    {
        public DbSet<t_User> Users { get; set; }
        public DbSet<t_Room> Roms { get; set; }
        public DbSet<t_Game> Games { get; set; }
        public DbSet<t_Card> Cards { get; set; }
        public DbSet<t_Side> Sides { get; set; }
        public DbSet<t_GameCard> GameCard { get; set; }

        public AppDBContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=GameUnoDB;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<t_Game>()
                .HasOne(g => g.Room)
                .WithMany(r => r.Games)
                .HasForeignKey(g => g.RoomId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<t_Card>()
                .HasOne(c => c.Ligth)
                .WithMany()
                .HasForeignKey(c => c.LigthId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<t_Card>()
                .HasOne(c => c.Dark)
                .WithMany()
                .HasForeignKey(c => c.DarkId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<t_GameCard>()
                .HasKey(gc => new { gc.GameId, gc.CardId });

            modelBuilder.Entity<t_GameCard>()
                .HasOne(gc => gc.Game)
                .WithMany(g => g.Cards)
                .HasForeignKey(gc => gc.GameId);

            modelBuilder.Entity<t_GameCard>()
                .HasOne(gc => gc.Card)
                .WithMany(c => c.Game)
                .HasForeignKey(gc => gc.CardId);
        }
    }
}

public class t_User
{
    public int Id { get; set; }
    public string Login { get; set; }
}

public class t_Room
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string UsersIds { get; set; }
    public int State { get; set; }
    public ICollection<t_Game> Games { get; set; }
}

public class t_Game
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public t_Room Room { get; set; }
    public int CountPlayers { get; set; }
    public int ActivePlayer { get; set; }
    public GameStatus State { get; set; }
    public Side Side { get; set; }
    public Direction Direction { get; set; }
    public Color Color { get; set; }
    public int? LastCardPlayed { get; set; }
    public int? NextCardInDeck { get; set; }
    public ICollection<t_GameCard> Cards { get; set; }
}

public class t_GameCard
{
    public int GameId { get; set; }
    public t_Game Game { get; set; }

    public int CardId { get; set; }
    public t_Card Card { get; set; }
}

public class t_Card
{
    public int Id { get; set; }

    [ForeignKey("Ligth")]
    public int LigthId { get; set; }
    public t_Side Ligth { get; set; }

    [ForeignKey("Dark")]
    public int DarkId { get; set; }
    public t_Side Dark { get; set; }
    public ICollection<t_GameCard> Game { get; set; }


    public t_Card() { }
    public t_Card SetLigth(t_Side side)
    {
        Ligth = side;
        return this;
    }
    public t_Card SetDark(t_Side side)
    {
        Dark = side;
        return this;
    }
    public t_Side Get(Side side) => side == Side.Light ? Ligth : Dark;

    public string ToString(Side side)
    {
        return (side == Side.Light ? $"{{ {Ligth} }}" : $"{{ {Dark} }}");
    }

    public override string ToString()
    {
        return $"{{ {Id}, {Ligth}, {Dark} }}";
    }
}

public class t_Side
{
    public int Id { get; set; }
    public Color Color { get; set; }
    public Action Action { get; set; }
    public int Value { get; set; }

    public t_Side(Color color = Color.None, Action action = Action.None, int value = -1)
    {
        Color = color;
        Action = action;
        Value = value;
    }

    public override string ToString()
    {
        return $"{Id}, {Color}, {Action}, {Value}";
    }
}