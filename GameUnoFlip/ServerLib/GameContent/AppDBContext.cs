using GameCore.Classes;
using GameCore.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Action = GameCore.Enums.Action;

namespace ServerLib.GameContent
{
    public class AppDBContext : DbContext
    {
        public DbSet<t_User> Users { get; set; }
        public DbSet<t_Card> Cards { get; set; }
        public DbSet<t_Side> Sides { get; set; }

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
        }
    }
}

public class t_User
{
    public int Id { get; set; }
    public string Login { get; set; }
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

    public Card ToCard()
    {
        return new Card(Id, Ligth.ToCardSide(), Dark.ToCardSide());
    }
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
    public CardSide ToCardSide()
    {
        return new CardSide(Color, Action, Value);
    }
    public override string ToString()
    {
        return $"{Id}, {Color}, {Action}, {Value}";
    }
}