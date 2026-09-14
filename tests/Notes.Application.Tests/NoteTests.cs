using Notes.Domain.Entities;
using Xunit;

namespace Notes.Application.Tests;

public sealed class NoteTests
{
    [Fact]
    public void Create_MembuatNoteDenganTimestampYangSama()
    {
        var note = Note.Create("Belajar .NET", "Pelajari ASP.NET Core.");

        Assert.NotEqual(Guid.Empty, note.Id);
        Assert.Equal("Belajar .NET", note.Title);
        Assert.Equal("Pelajari ASP.NET Core.", note.Content);
        Assert.Equal(note.CreatedAt, note.UpdatedAt);
    }

    [Fact]
    public void Create_MenolakTitleKosong()
    {
        var exception = Assert.Throws<ArgumentException>(() => Note.Create("", "Isi note"));

        Assert.Equal("title", exception.ParamName);
    }

    [Fact]
    public void Update_MengubahIsiDanUpdatedAt()
    {
        var note = Note.Create("Judul lama", "Isi lama");
        var createdAt = note.CreatedAt;

        note.Update("Judul baru", "Isi baru");

        Assert.Equal("Judul baru", note.Title);
        Assert.Equal("Isi baru", note.Content);
        Assert.True(note.UpdatedAt >= createdAt);
    }
}
