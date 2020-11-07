using Messenger_API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace Messenger_API.Data
{
    public class MessageContext : DbContext
    {
        public MessageContext(DbContextOptions<MessageContext> options) : base(options)
        { }

        public DbSet<Conversation>  Conversations{ get; set; }
        public DbSet<ConversationAdmin> ConversationAdmins { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<FriendName> FriendNames { get; set; }
        public DbSet<MessageContent> MessageContents { get; set; }
        public DbSet<Packet> Packets { get; set; }
        public DbSet<PacketContent> PacketContents { get; set; }
        public DbSet<SmallUser> SmallUsers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //SmallUser & FriendName (many to one)
            modelBuilder.Entity<SmallUser>()
                .HasKey(p => p.UserId);
            modelBuilder.Entity<FriendName>()
                .HasKey(p => new { p.FriendId, p.UserId });

            modelBuilder.Entity<FriendName>()
                .HasOne(s => s.SmallUser)
                .WithMany(f => f.FriendNames)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            //FriendName & Friend (one to many)
            modelBuilder.Entity<Friend>()
                .HasKey(p => p.FriendId);

            modelBuilder.Entity<FriendName>()
                .HasOne(f => f.Friend)
                .WithMany(fn => fn.FriendNames)
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);


            //SmallUser & MessageContent (many to one)
            modelBuilder.Entity<MessageContent>()
                .HasKey(p => p.MessageId);

            modelBuilder.Entity<MessageContent>()
                .HasOne(s => s.SmallUser)
                .WithMany(m => m.MessageContents)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            //MessageContent & PacketContent (one to one)
            modelBuilder.Entity<PacketContent>()
                .HasKey(p => p.MessageId);

            modelBuilder.Entity<PacketContent>()
                .HasOne(m => m.MessageContent)
                .WithOne(p => p.PacketContent)
                .HasForeignKey<PacketContent>(m => m.MessageId)
                .OnDelete(DeleteBehavior.Restrict);


            //PacketContent & Packet (one to many)
            modelBuilder.Entity<Packet>()
                .HasKey(p => p.PacketId);

            modelBuilder.Entity<PacketContent>()
                .HasOne(p => p.Packet)
                .WithMany(pc => pc.PacketContents)
                .HasForeignKey(p => p.PacketId)
                .OnDelete(DeleteBehavior.Restrict);


            //SmallUser & ConversationAdmin (many to one)
            modelBuilder.Entity<ConversationAdmin>()
                .HasKey(p => new { p.ConversationId, p.UserId });

            modelBuilder.Entity<ConversationAdmin>()
                .HasOne(s => s.SmallUser)
                .WithMany(a => a.ConversationAdmins)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            //SmallUser & Conversation (many to one)
            modelBuilder.Entity<Conversation>()
                .HasKey(p => p.ConversationId);

            modelBuilder.Entity<Conversation>()
                .HasOne(s => s.SmallUser)
                .WithMany(c => c.Conversations)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            //Conversation & ConversationAdmin (many to one)
            modelBuilder.Entity<ConversationAdmin>()
                .HasOne(c => c.Conversation)
                .WithMany(a => a.ConversationAdmins)
                .HasForeignKey(c => c.ConversationId)
                .OnDelete(DeleteBehavior.Restrict);


            //Conversation & Packet (many to one)
            modelBuilder.Entity<Packet>()
                .HasOne(c => c.Conversation)
                .WithMany(p => p.Packets)
                .HasForeignKey(c => c.ConversationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
