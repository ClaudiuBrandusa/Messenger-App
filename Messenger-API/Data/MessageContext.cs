using Messenger_API.Entities;
using Messenger_API.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
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
        {
        }

        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationMember> ConversationMembers { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<FriendName> FriendNames { get; set; }
        public DbSet<MessageContent> MessageContents { get; set; }
        public DbSet<Packet> Packets { get; set; }
        public DbSet<PacketContent> PacketContents { get; set; }
        public DbSet<SmallUser> SmallUsers { get; set; }
        public DbSet<ImageProfile> ImageProfiles { get; set; }
        public DbSet<ConversationDetail> ConversationDetails { get; set; }


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

            //SmallUser & ConversationMember (one to many)

            modelBuilder.Entity<ConversationMember>()
                .HasOne(c => c.SmallUser)
                .WithMany(u => u.Conversations)
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<ConversationMember>()
                .HasKey(m => new { m.ConversationId, m.UserId });

            // Conversation

            modelBuilder.Entity<Conversation>()
                .HasKey(p => p.ConversationId);

            modelBuilder.Entity<Conversation>()
                .HasMany(p => p.Packets)
                .WithOne(c => c.Conversation)
                .HasForeignKey(p => p.ConversationId);

            //Conversation & Packet (many to one)
            modelBuilder.Entity<Packet>()
                .HasOne(c => c.Conversation)
                .WithMany(p => p.Packets)
                .HasForeignKey(c => c.ConversationId)
                //.HasPrincipalKey(c => c.ConversationId)
                .OnDelete(DeleteBehavior.Restrict);

            //Profile & SmallUser (one to one)
            modelBuilder.Entity<ImageProfile>()
                .HasKey(p => p.UserId);

            modelBuilder.Entity<ImageProfile>()
                .HasOne(s => s.SmallUser)
                .WithOne(p => p.ImageProfile)
                .HasForeignKey<ImageProfile>(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //ConversationDetail & Conversation (one to one)
            modelBuilder.Entity<ConversationDetail>()
                .HasKey(p => p.ConversationId);

            modelBuilder.Entity<Conversation>()
                .HasOne(s => s.ConversationDetail)
                .WithOne(p => p.Conversation)
                .HasForeignKey<ConversationDetail>(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Restrict);

            // BlockedContact

            modelBuilder.Entity<BlockedContact>()
                .HasKey(b => b.ConversationId);
    
            //Conversation & Blocked Contact
            modelBuilder.Entity<Conversation>()
                .HasOne(b => b.BlockedContact)
                .WithOne(c => c.Conversation)
                .HasForeignKey<BlockedContact>(b => b.ConversationId);

            //SmallUser & Blocked Contact (many to one)

            modelBuilder.Entity<SmallUser>()
                .HasMany(u => u.BlockedContacts)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId);
        }
    }
}