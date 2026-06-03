using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace pp.Models
{
    [Table("contacts")]
    public class Contact
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("birthday")]
        public DateTime? Birthday { get; set; }

        public ICollection<Task>? Tasks { get; set; }
    }

    [Table("tasks")]
    public class Task
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("deadline")]
        public DateTime? Deadline { get; set; }

        [Column("is_completed")]
        public bool IsCompleted { get; set; }

        [Column("contact_id")]
        public int? ContactId { get; set; }

        public Contact? Contact { get; set; }
    }

    [Table("notes")]
    public class Note
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("content")]
        public string? Content { get; set; }

        [Column("color_tag")]
        public string? ColorTag { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }

        public Category? Category { get; set; }
    }

    [Table("categories")]
    public class Category
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        public ICollection<Note>? Notes { get; set; }
    }
}