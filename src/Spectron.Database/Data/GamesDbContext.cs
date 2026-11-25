using Microsoft.EntityFrameworkCore;
using OldBit.Spectron.Database.Models;
using File = OldBit.Spectron.Database.Models.File;

namespace OldBit.Spectron.Database.Data;

public partial class GamesDbContext(DbContextOptions<GamesDbContext> options) : DbContext(options)
{
    public virtual DbSet<Alias> Aliases { get; set; }

    public virtual DbSet<Article> Articles { get; set; }

    public virtual DbSet<Articletype> Articletypes { get; set; }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Availabletype> Availabletypes { get; set; }

    public virtual DbSet<Booktypein> Booktypeins { get; set; }

    public virtual DbSet<Casetype> Casetypes { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Content> Contents { get; set; }

    public virtual DbSet<Contenttype> Contenttypes { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<Download> Downloads { get; set; }

    public virtual DbSet<Entry> Entries { get; set; }

    public virtual DbSet<Extension> Extensions { get; set; }

    public virtual DbSet<Feature> Features { get; set; }

    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<Filetype> Filetypes { get; set; }

    public virtual DbSet<Genretype> Genretypes { get; set; }

    public virtual DbSet<Host> Hosts { get; set; }

    public virtual DbSet<Issue> Issues { get; set; }

    public virtual DbSet<Label> Labels { get; set; }

    public virtual DbSet<Labeltype> Labeltypes { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<License> Licenses { get; set; }

    public virtual DbSet<Licensetype> Licensetypes { get; set; }

    public virtual DbSet<Machinetype> Machinetypes { get; set; }

    public virtual DbSet<Magazine> Magazines { get; set; }

    public virtual DbSet<Magref> Magrefs { get; set; }

    public virtual DbSet<Magreflink> Magreflinks { get; set; }

    public virtual DbSet<Magtype> Magtypes { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Note> Notes { get; set; }

    public virtual DbSet<Notetype> Notetypes { get; set; }

    public virtual DbSet<Nvg> Nvgs { get; set; }

    public virtual DbSet<Origintype> Origintypes { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Permissiontype> Permissiontypes { get; set; }

    public virtual DbSet<Platform> Platforms { get; set; }

    public virtual DbSet<Port> Ports { get; set; }

    public virtual DbSet<Prefix> Prefixes { get; set; }

    public virtual DbSet<Prefixexempt> Prefixexempts { get; set; }

    public virtual DbSet<Publisher> Publishers { get; set; }

    public virtual DbSet<Referencetype> Referencetypes { get; set; }

    public virtual DbSet<Relatedlicense> Relatedlicenses { get; set; }

    public virtual DbSet<Relation> Relations { get; set; }

    public virtual DbSet<Relationtype> Relationtypes { get; set; }

    public virtual DbSet<Release> Releases { get; set; }

    public virtual DbSet<Remake> Remakes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Roletype> Roletypes { get; set; }

    public virtual DbSet<Schemetype> Schemetypes { get; set; }

    public virtual DbSet<Score> Scores { get; set; }

    public virtual DbSet<Scrap> Scraps { get; set; }

    public virtual DbSet<SearchByAlias> SearchByAliases { get; set; }

    public virtual DbSet<SearchByAuthor> SearchByAuthors { get; set; }

    public virtual DbSet<SearchByIssue> SearchByIssues { get; set; }

    public virtual DbSet<SearchByMagazine> SearchByMagazines { get; set; }

    public virtual DbSet<SearchByMagref> SearchByMagrefs { get; set; }

    public virtual DbSet<SearchByName> SearchByNames { get; set; }

    public virtual DbSet<SearchByOrigin> SearchByOrigins { get; set; }

    public virtual DbSet<SearchByPublisher> SearchByPublishers { get; set; }

    public virtual DbSet<SearchByTitle> SearchByTitles { get; set; }

    public virtual DbSet<Sourcetype> Sourcetypes { get; set; }

    public virtual DbSet<SpexAuthor> SpexAuthors { get; set; }

    public virtual DbSet<SpexEntry> SpexEntries { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Tagtype> Tagtypes { get; set; }

    public virtual DbSet<Tool> Tools { get; set; }

    public virtual DbSet<Tooltype> Tooltypes { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    public virtual DbSet<Topictype> Topictypes { get; set; }

    public virtual DbSet<Webref> Webrefs { get; set; }

    public virtual DbSet<Website> Websites { get; set; }

    public virtual DbSet<ZxsrAward> ZxsrAwards { get; set; }

    public virtual DbSet<ZxsrCaption> ZxsrCaptions { get; set; }

    public virtual DbSet<ZxsrReview> ZxsrReviews { get; set; }

    public virtual DbSet<ZxsrScore> ZxsrScores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alias>(entity =>
        {
            entity.HasKey(e => new { e.EntryId, e.ReleaseSeq, e.LanguageId, e.Title });

            entity.ToTable("aliases");

            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.ReleaseSeq)
                .HasColumnType("smallint(6)")
                .HasColumnName("release_seq");
            entity.Property(e => e.LanguageId)
                .UseCollation("rtrim")
                .HasColumnType("char(2)")
                .HasColumnName("language_id");
            entity.Property(e => e.Title)
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("title");

            entity.HasOne(d => d.Entry).WithMany(p => p.Aliases)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Language).WithMany(p => p.Aliases)
                .HasForeignKey(d => d.LanguageId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Release).WithMany(p => p.Aliases)
                .HasForeignKey(d => new { d.EntryId, d.ReleaseSeq })
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => new { e.LabelId, e.Link });

            entity.ToTable("articles");

            entity.Property(e => e.LabelId)
                .HasColumnType("int(11)")
                .HasColumnName("label_id");
            entity.Property(e => e.Link)
                .UseCollation("rtrim")
                .HasColumnType("varchar(200)")
                .HasColumnName("link");
            entity.Property(e => e.ArticletypeId)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("articletype_id");
            entity.Property(e => e.DateYear)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("date_year");
            entity.Property(e => e.LanguageId)
                .UseCollation("rtrim")
                .HasColumnType("char(2)")
                .HasColumnName("language_id");
            entity.Property(e => e.Title)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(200)")
                .HasColumnName("title");
            entity.Property(e => e.Writer)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(200)")
                .HasColumnName("writer");

            entity.HasOne(d => d.Articletype).WithMany(p => p.Articles)
                .HasForeignKey(d => d.ArticletypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Label).WithMany(p => p.Articles)
                .HasForeignKey(d => d.LabelId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Language).WithMany(p => p.Articles)
                .HasForeignKey(d => d.LanguageId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Articletype>(entity =>
        {
            entity.ToTable("articletypes");

            entity.HasIndex(e => e.Text, "IX_articletypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => new { e.EntryId, e.LabelId });

            entity.ToTable("authors");

            entity.HasIndex(e => new { e.EntryId, e.AuthorSeq }, "IX_authors_entry_id_author_seq").IsUnique();

            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.LabelId)
                .HasColumnType("int(11)")
                .HasColumnName("label_id");
            entity.Property(e => e.AuthorSeq)
                .HasDefaultValue((short)1)
                .HasColumnType("smallint(6)")
                .HasColumnName("author_seq");
            entity.Property(e => e.TeamId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("team_id");

            entity.HasOne(d => d.Entry).WithMany(p => p.Authors)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Label).WithMany(p => p.AuthorLabels)
                .HasForeignKey(d => d.LabelId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Team).WithMany(p => p.AuthorTeams).HasForeignKey(d => d.TeamId);
        });

        modelBuilder.Entity<Availabletype>(entity =>
        {
            entity.ToTable("availabletypes");

            entity.HasIndex(e => e.Text, "IX_availabletypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Booktypein>(entity =>
        {
            entity.HasKey(e => new { e.EntryId, e.BookId, e.Installment, e.Volume, e.Page });

            entity.ToTable("booktypeins");

            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.BookId)
                .HasColumnType("int(11)")
                .HasColumnName("book_id");
            entity.Property(e => e.Installment)
                .HasColumnType("smallint(6)")
                .HasColumnName("installment");
            entity.Property(e => e.Volume)
                .HasColumnType("smallint(6)")
                .HasColumnName("volume");
            entity.Property(e => e.Page)
                .HasColumnType("smallint(6)")
                .HasColumnName("page");
            entity.Property(e => e.IsOriginal)
                .HasColumnType("tinyint(1)")
                .HasColumnName("is_original");

            entity.HasOne(d => d.Book).WithMany(p => p.BooktypeinBooks)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Entry).WithMany(p => p.BooktypeinEntries)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Casetype>(entity =>
        {
            entity.ToTable("casetypes");

            entity.HasIndex(e => e.Text, "IX_casetypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");

            entity.HasIndex(e => e.Text, "IX_categories_text").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("smallint(6)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(200)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Content>(entity =>
        {
            entity.HasKey(e => new { e.ContainerId, e.MediaSeq, e.MediaSide, e.ProgSeq });

            entity.ToTable("contents");

            entity.Property(e => e.ContainerId)
                .HasColumnType("int(11)")
                .HasColumnName("container_id");
            entity.Property(e => e.MediaSeq)
                .HasColumnType("smallint(6)")
                .HasColumnName("media_seq");
            entity.Property(e => e.MediaSide)
                .HasDefaultValue("0")
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("media_side");
            entity.Property(e => e.ProgSeq)
                .HasColumnType("smallint(6)")
                .HasColumnName("prog_seq");
            entity.Property(e => e.Alias)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("alias");
            entity.Property(e => e.ContenttypeId)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("contenttype_id");
            entity.Property(e => e.EntryId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.IsOriginal)
                .HasColumnType("tinyint(1)")
                .HasColumnName("is_original");
            entity.Property(e => e.LabelId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("label_id");

            entity.HasOne(d => d.Container).WithMany(p => p.ContentContainers)
                .HasForeignKey(d => d.ContainerId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Contenttype).WithMany(p => p.Contents)
                .HasForeignKey(d => d.ContenttypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Entry).WithMany(p => p.ContentEntries).HasForeignKey(d => d.EntryId);

            entity.HasOne(d => d.Label).WithMany(p => p.Contents).HasForeignKey(d => d.LabelId);
        });

        modelBuilder.Entity<Contenttype>(entity =>
        {
            entity.ToTable("contenttypes");

            entity.HasIndex(e => e.Text, "IX_contenttypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("countries");

            entity.HasIndex(e => e.Text, "IX_countries_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(2)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.ToTable("currencies");

            entity.HasIndex(e => e.Name, "IX_currencies_name").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(3)")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("name");
            entity.Property(e => e.Prefix)
                .HasColumnType("tinyint(1)")
                .HasColumnName("prefix");
            entity.Property(e => e.Symbol)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(20)")
                .HasColumnName("symbol");
        });

        modelBuilder.Entity<Download>(entity =>
        {
            entity.ToTable("downloads");

            entity.HasIndex(e => new { e.FileLink, e.EntryId }, "IX_downloads_file_link_entry_id").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.CasetypeId)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("casetype_id");
            entity.Property(e => e.Comments)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("comments");
            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.FileBarcode)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("file_barcode");
            entity.Property(e => e.FileCode)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("file_code");
            entity.Property(e => e.FileDate)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("file_date");
            entity.Property(e => e.FileDl)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(150)")
                .HasColumnName("file_dl");
            entity.Property(e => e.FileLink)
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("file_link");
            entity.Property(e => e.FileMd5)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(32)")
                .HasColumnName("file_md5");
            entity.Property(e => e.FileSize)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("file_size");
            entity.Property(e => e.FiletypeId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("filetype_id");
            entity.Property(e => e.IsDemo)
                .HasColumnType("tinyint(1)")
                .HasColumnName("is_demo");
            entity.Property(e => e.LanguageId)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("char(2)")
                .HasColumnName("language_id");
            entity.Property(e => e.MachinetypeId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("tinyint(4)")
                .HasColumnName("machinetype_id");
            entity.Property(e => e.ReleaseSeq)
                .HasColumnType("smallint(6)")
                .HasColumnName("release_seq");
            entity.Property(e => e.ReleaseYear)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("release_year");
            entity.Property(e => e.SchemetypeId)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("char(2)")
                .HasColumnName("schemetype_id");
            entity.Property(e => e.ScrBorder)
                .HasDefaultValue((byte)7)
                .HasColumnType("tinyint(1)")
                .HasColumnName("scr_border");
            entity.Property(e => e.SourcetypeId)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("sourcetype_id");

            entity.HasOne(d => d.Casetype).WithMany(p => p.Downloads).HasForeignKey(d => d.CasetypeId);

            entity.HasOne(d => d.Entry).WithMany(p => p.Downloads)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Filetype).WithMany(p => p.Downloads)
                .HasForeignKey(d => d.FiletypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Language).WithMany(p => p.Downloads).HasForeignKey(d => d.LanguageId);

            entity.HasOne(d => d.Machinetype).WithMany(p => p.Downloads).HasForeignKey(d => d.MachinetypeId);

            entity.HasOne(d => d.Schemetype).WithMany(p => p.Downloads).HasForeignKey(d => d.SchemetypeId);

            entity.HasOne(d => d.Sourcetype).WithMany(p => p.Downloads).HasForeignKey(d => d.SourcetypeId);

            entity.HasOne(d => d.Release).WithMany(p => p.Downloads)
                .HasForeignKey(d => new { d.EntryId, d.ReleaseSeq })
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Entry>(entity =>
        {
            entity.ToTable("entries");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.AvailabletypeId)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("availabletype_id");
            entity.Property(e => e.GenretypeId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("tinyint(4)")
                .HasColumnName("genretype_id");
            entity.Property(e => e.IsXrated)
                .HasColumnType("tinyint(1)")
                .HasColumnName("is_xrated");
            entity.Property(e => e.IssueId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("issue_id");
            entity.Property(e => e.LanguageId)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("char(2)")
                .HasColumnName("language_id");
            entity.Property(e => e.MachinetypeId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("tinyint(4)")
                .HasColumnName("machinetype_id");
            entity.Property(e => e.MaxPlayers)
                .HasDefaultValue((byte)1)
                .HasColumnType("tinyint(4)")
                .HasColumnName("max_players");
            entity.Property(e => e.SpotGenretypeId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("tinyint(4)")
                .HasColumnName("spot_genretype_id");
            entity.Property(e => e.Title)
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("title");
            entity.Property(e => e.WithoutInlay)
                .HasColumnType("tinyint(1)")
                .HasColumnName("without_inlay");
            entity.Property(e => e.WithoutLoadScreen)
                .HasColumnType("tinyint(1)")
                .HasColumnName("without_load_screen");

            entity.HasOne(d => d.Availabletype).WithMany(p => p.Entries).HasForeignKey(d => d.AvailabletypeId);

            entity.HasOne(d => d.Genretype).WithMany(p => p.EntryGenretypes).HasForeignKey(d => d.GenretypeId);

            entity.HasOne(d => d.Issue).WithMany(p => p.Entries).HasForeignKey(d => d.IssueId);

            entity.HasOne(d => d.Language).WithMany(p => p.Entries).HasForeignKey(d => d.LanguageId);

            entity.HasOne(d => d.Machinetype).WithMany(p => p.Entries).HasForeignKey(d => d.MachinetypeId);

            entity.HasOne(d => d.SpotGenretype).WithMany(p => p.EntrySpotGenretypes).HasForeignKey(d => d.SpotGenretypeId);
        });

        modelBuilder.Entity<Extension>(entity =>
        {
            entity.HasKey(e => e.Ext);

            entity.ToTable("extensions");

            entity.HasIndex(e => e.Text, "IX_extensions_text").IsUnique();

            entity.Property(e => e.Ext)
                .UseCollation("rtrim")
                .HasColumnType("varchar(15)")
                .HasColumnName("ext");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Feature>(entity =>
        {
            entity.ToTable("features");

            entity.HasIndex(e => new { e.Name, e.Version, e.LabelId }, "IX_features_name_version_label_id").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Label2Id)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("label2_id");
            entity.Property(e => e.LabelId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("label_id");
            entity.Property(e => e.Name)
                .UseCollation("rtrim")
                .HasColumnType("varchar(150)")
                .HasColumnName("name");
            entity.Property(e => e.Version)
                .HasColumnType("tinyint(4)")
                .HasColumnName("version");

            entity.HasOne(d => d.Label2).WithMany(p => p.FeatureLabel2s).HasForeignKey(d => d.Label2Id);

            entity.HasOne(d => d.Label).WithMany(p => p.FeatureLabels).HasForeignKey(d => d.LabelId);
        });

        modelBuilder.Entity<File>(entity =>
        {
            entity.ToTable("files");

            entity.HasIndex(e => new { e.IssueId, e.FileLink }, "IX_files_issue_id_file_link").IsUnique();

            entity.HasIndex(e => new { e.LabelId, e.FileLink }, "IX_files_label_id_file_link").IsUnique();

            entity.HasIndex(e => new { e.ToolId, e.FileLink }, "IX_files_tool_id_file_link").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Comments)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("comments");
            entity.Property(e => e.FileDate)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("file_date");
            entity.Property(e => e.FileLink)
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("file_link");
            entity.Property(e => e.FileMd5)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(32)")
                .HasColumnName("file_md5");
            entity.Property(e => e.FileSize)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("file_size");
            entity.Property(e => e.FiletypeId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("filetype_id");
            entity.Property(e => e.IssueId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("issue_id");
            entity.Property(e => e.LabelId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("label_id");
            entity.Property(e => e.ToolId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("tool_id");

            entity.HasOne(d => d.Filetype).WithMany(p => p.Files)
                .HasForeignKey(d => d.FiletypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Issue).WithMany(p => p.Files).HasForeignKey(d => d.IssueId);

            entity.HasOne(d => d.Label).WithMany(p => p.Files).HasForeignKey(d => d.LabelId);

            entity.HasOne(d => d.Tool).WithMany(p => p.Files).HasForeignKey(d => d.ToolId);
        });

        modelBuilder.Entity<Filetype>(entity =>
        {
            entity.ToTable("filetypes");

            entity.HasIndex(e => e.Text, "IX_filetypes_text").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("tinyint(4)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Genretype>(entity =>
        {
            entity.ToTable("genretypes");

            entity.HasIndex(e => e.Text, "IX_genretypes_text").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("tinyint(4)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Host>(entity =>
        {
            entity.ToTable("hosts");

            entity.HasIndex(e => e.Title, "IX_hosts_title").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("tinyint(4)")
                .HasColumnName("id");
            entity.Property(e => e.Admin)
                .UseCollation("rtrim")
                .HasColumnType("varchar(150)")
                .HasColumnName("admin");
            entity.Property(e => e.Link)
                .UseCollation("rtrim")
                .HasColumnType("varchar(150)")
                .HasColumnName("link");
            entity.Property(e => e.MagazineId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("magazine_id");
            entity.Property(e => e.Title)
                .UseCollation("rtrim")
                .HasColumnType("varchar(150)")
                .HasColumnName("title");

            entity.HasOne(d => d.Magazine).WithMany(p => p.Hosts).HasForeignKey(d => d.MagazineId);
        });

        modelBuilder.Entity<Issue>(entity =>
        {
            entity.ToTable("issues");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.ArchiveMask)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("archive_mask");
            entity.Property(e => e.CoverPage)
                .HasDefaultValue((short)1)
                .HasColumnType("smallint(6)")
                .HasColumnName("cover_page");
            entity.Property(e => e.CurrencyId)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("char(3)")
                .HasColumnName("currency_id");
            entity.Property(e => e.DateDay)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("date_day");
            entity.Property(e => e.DateMonth)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("date_month");
            entity.Property(e => e.DateYear)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("date_year");
            entity.Property(e => e.Label2Id)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("label2_id");
            entity.Property(e => e.LabelId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("label_id");
            entity.Property(e => e.LinkMask)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("link_mask");
            entity.Property(e => e.MagazineId)
                .HasColumnType("smallint(6)")
                .HasColumnName("magazine_id");
            entity.Property(e => e.Number)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("number");
            entity.Property(e => e.ParentId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("parent_id");
            entity.Property(e => e.Price)
                .HasDefaultValueSql("NULL")
                .HasColumnType("decimal(9,2)")
                .HasColumnName("price");
            entity.Property(e => e.Special)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("special");
            entity.Property(e => e.Supplement)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("supplement");
            entity.Property(e => e.TranslationMask)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("translation_mask");
            entity.Property(e => e.Volume)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("volume");

            entity.HasOne(d => d.Currency).WithMany(p => p.Issues).HasForeignKey(d => d.CurrencyId);

            entity.HasOne(d => d.Label2).WithMany(p => p.IssueLabel2s).HasForeignKey(d => d.Label2Id);

            entity.HasOne(d => d.Label).WithMany(p => p.IssueLabels).HasForeignKey(d => d.LabelId);

            entity.HasOne(d => d.Magazine).WithMany(p => p.Issues)
                .HasForeignKey(d => d.MagazineId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent).HasForeignKey(d => d.ParentId);
        });

        modelBuilder.Entity<Label>(entity =>
        {
            entity.ToTable("labels");

            entity.HasIndex(e => e.Name, "IX_labels_name").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Country2Id)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("char(2)")
                .HasColumnName("country2_id");
            entity.Property(e => e.CountryId)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("char(2)")
                .HasColumnName("country_id");
            entity.Property(e => e.Deceased)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(200)")
                .HasColumnName("deceased");
            entity.Property(e => e.FromId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("from_id");
            entity.Property(e => e.LabeltypeId)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("labeltype_id");
            entity.Property(e => e.LinkSite)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(200)")
                .HasColumnName("link_site");
            entity.Property(e => e.LinkWikipedia)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(200)")
                .HasColumnName("link_wikipedia");
            entity.Property(e => e.Name)
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("name");
            entity.Property(e => e.OwnerId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("owner_id");
            entity.Property(e => e.WasRenamed)
                .HasColumnType("tinyint(1)")
                .HasColumnName("was_renamed");

            entity.HasOne(d => d.Country2).WithMany(p => p.LabelCountry2s).HasForeignKey(d => d.Country2Id);

            entity.HasOne(d => d.Country).WithMany(p => p.LabelCountries).HasForeignKey(d => d.CountryId);

            entity.HasOne(d => d.From).WithMany(p => p.InverseFrom).HasForeignKey(d => d.FromId);

            entity.HasOne(d => d.Labeltype).WithMany(p => p.Labels).HasForeignKey(d => d.LabeltypeId);

            entity.HasOne(d => d.Owner).WithMany(p => p.InverseOwner).HasForeignKey(d => d.OwnerId);
        });

        modelBuilder.Entity<Labeltype>(entity =>
        {
            entity.ToTable("labeltypes");

            entity.HasIndex(e => e.Text, "IX_labeltypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.ToTable("languages");

            entity.HasIndex(e => e.Text, "IX_languages_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(2)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<License>(entity =>
        {
            entity.ToTable("licenses");

            entity.HasIndex(e => new { e.Name, e.LicensetypeId }, "IX_licenses_name_licensetype_id").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Comments)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(500)")
                .HasColumnName("comments");
            entity.Property(e => e.LicensetypeId)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("licensetype_id");
            entity.Property(e => e.LinkSite)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(200)")
                .HasColumnName("link_site");
            entity.Property(e => e.LinkWikipedia)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(200)")
                .HasColumnName("link_wikipedia");
            entity.Property(e => e.Name)
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("name");

            entity.HasOne(d => d.Licensetype).WithMany(p => p.Licenses)
                .HasForeignKey(d => d.LicensetypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasMany(d => d.Labels).WithMany(p => p.Licenses)
                .UsingEntity<Dictionary<string, object>>(
                    "Licensor",
                    r => r.HasOne<Label>().WithMany()
                        .HasForeignKey("LabelId")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    l => l.HasOne<License>().WithMany()
                        .HasForeignKey("LicenseId")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey("LicenseId", "LabelId");
                        j.ToTable("licensors");
                        j.IndexerProperty<int>("LicenseId")
                            .HasColumnType("int(11)")
                            .HasColumnName("license_id");
                        j.IndexerProperty<int>("LabelId")
                            .HasColumnType("int(11)")
                            .HasColumnName("label_id");
                    });
        });

        modelBuilder.Entity<Licensetype>(entity =>
        {
            entity.ToTable("licensetypes");

            entity.HasIndex(e => e.Text, "IX_licensetypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Machinetype>(entity =>
        {
            entity.ToTable("machinetypes");

            entity.HasIndex(e => e.Text, "IX_machinetypes_text").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("tinyint(4)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Magazine>(entity =>
        {
            entity.ToTable("magazines");

            entity.HasIndex(e => e.Name, "IX_magazines_name").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("smallint(6)")
                .HasColumnName("id");
            entity.Property(e => e.ArchiveMask)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("archive_mask");
            entity.Property(e => e.CountryId)
                .UseCollation("rtrim")
                .HasColumnType("char(2)")
                .HasColumnName("country_id");
            entity.Property(e => e.LanguageId)
                .UseCollation("rtrim")
                .HasColumnType("char(2)")
                .HasColumnName("language_id");
            entity.Property(e => e.LinkMask)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("link_mask");
            entity.Property(e => e.LinkSite)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(200)")
                .HasColumnName("link_site");
            entity.Property(e => e.MagtypeId)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("magtype_id");
            entity.Property(e => e.Name)
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("name");
            entity.Property(e => e.TopicId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("topic_id");
            entity.Property(e => e.TranslationMask)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("translation_mask");

            entity.HasOne(d => d.Country).WithMany(p => p.Magazines)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Language).WithMany(p => p.Magazines)
                .HasForeignKey(d => d.LanguageId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Magtype).WithMany(p => p.Magazines)
                .HasForeignKey(d => d.MagtypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Topic).WithMany(p => p.Magazines).HasForeignKey(d => d.TopicId);
        });

        modelBuilder.Entity<Magref>(entity =>
        {
            entity.ToTable("magrefs");

            entity.HasIndex(e => new { e.LabelId, e.IssueId, e.Page, e.ReferencetypeId }, "IX_magrefs_label_id_issue_id_page_referencetype_id").IsUnique();

            entity.HasIndex(e => new { e.TopicId, e.IssueId, e.Page, e.ReferencetypeId }, "IX_magrefs_topic_id_issue_id_page_referencetype_id").IsUnique();

            entity.HasIndex(e => new { e.EntryId, e.IssueId, e.Page, e.ReferencetypeId, e.ScoreGroup }, "IX_magrefs_entry_id_issue_id_page_referencetype_id_score_group").IsUnique();

            entity.HasIndex(e => new { e.IssueId, e.Page, e.ReferencetypeId, e.EntryId, e.LabelId, e.TopicId, e.ScoreGroup }, "IX_magrefs_issue_id_page_referencetype_id_entry_id_label_id_topic_id_score_group").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.AwardId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("tinyint(4)")
                .HasColumnName("award_id");
            entity.Property(e => e.EntryId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.IsOriginal)
                .HasColumnType("tinyint(1)")
                .HasColumnName("is_original");
            entity.Property(e => e.IssueId)
                .HasColumnType("int(11)")
                .HasColumnName("issue_id");
            entity.Property(e => e.LabelId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("label_id");
            entity.Property(e => e.Page)
                .HasColumnType("smallint(6)")
                .HasColumnName("page");
            entity.Property(e => e.ReferencetypeId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("referencetype_id");
            entity.Property(e => e.ReviewId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("review_id");
            entity.Property(e => e.ScoreGroup)
                .HasDefaultValue("")
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("score_group");
            entity.Property(e => e.TopicId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("topic_id");

            entity.HasOne(d => d.Award).WithMany(p => p.Magrefs).HasForeignKey(d => d.AwardId);

            entity.HasOne(d => d.Entry).WithMany(p => p.Magrefs).HasForeignKey(d => d.EntryId);

            entity.HasOne(d => d.Issue).WithMany(p => p.Magrefs)
                .HasForeignKey(d => d.IssueId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Label).WithMany(p => p.Magrefs).HasForeignKey(d => d.LabelId);

            entity.HasOne(d => d.Referencetype).WithMany(p => p.Magrefs)
                .HasForeignKey(d => d.ReferencetypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Review).WithMany(p => p.Magrefs).HasForeignKey(d => d.ReviewId);

            entity.HasOne(d => d.Topic).WithMany(p => p.Magrefs).HasForeignKey(d => d.TopicId);

            entity.HasMany(d => d.Features).WithMany(p => p.Magrefs)
                .UsingEntity<Dictionary<string, object>>(
                    "Magreffeat",
                    r => r.HasOne<Feature>().WithMany()
                        .HasForeignKey("FeatureId")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    l => l.HasOne<Magref>().WithMany()
                        .HasForeignKey("MagrefId")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey("MagrefId", "FeatureId");
                        j.ToTable("magreffeats");
                        j.IndexerProperty<int>("MagrefId")
                            .HasColumnType("int(11)")
                            .HasColumnName("magref_id");
                        j.IndexerProperty<int>("FeatureId")
                            .HasColumnType("int(11)")
                            .HasColumnName("feature_id");
                    });
        });

        modelBuilder.Entity<Magreflink>(entity =>
        {
            entity.HasKey(e => new { e.MagrefId, e.Link });

            entity.ToTable("magreflinks");

            entity.Property(e => e.MagrefId)
                .HasColumnType("int(11)")
                .HasColumnName("magref_id");
            entity.Property(e => e.Link)
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("link");
            entity.Property(e => e.HostId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("host_id");

            entity.HasOne(d => d.Host).WithMany(p => p.Magreflinks)
                .HasForeignKey(d => d.HostId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Magref).WithMany(p => p.Magreflinks)
                .HasForeignKey(d => d.MagrefId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Magtype>(entity =>
        {
            entity.ToTable("magtypes");

            entity.HasIndex(e => e.Text, "IX_magtypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => new { e.TagId, e.EntryId, e.CategoryId });

            entity.ToTable("members");

            entity.Property(e => e.TagId)
                .HasColumnType("int(11)")
                .HasColumnName("tag_id");
            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.CategoryId)
                .HasDefaultValue((short)1)
                .HasColumnType("smallint(6)")
                .HasColumnName("category_id");
            entity.Property(e => e.MemberSeq)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("member_seq");

            entity.HasOne(d => d.Category).WithMany(p => p.Members)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Entry).WithMany(p => p.Members)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Tag).WithMany(p => p.Members)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.ToTable("notes");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.EntryId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.LabelId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("label_id");
            entity.Property(e => e.NotetypeId)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("notetype_id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("mediumtext")
                .HasColumnName("text");

            entity.HasOne(d => d.Entry).WithMany(p => p.Notes).HasForeignKey(d => d.EntryId);

            entity.HasOne(d => d.Label).WithMany(p => p.Notes).HasForeignKey(d => d.LabelId);

            entity.HasOne(d => d.Notetype).WithMany(p => p.Notes)
                .HasForeignKey(d => d.NotetypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Notetype>(entity =>
        {
            entity.ToTable("notetypes");

            entity.HasIndex(e => e.Text, "IX_notetypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Nvg>(entity =>
        {
            entity.ToTable("nvgs");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Comments)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(500)")
                .HasColumnName("comments");
            entity.Property(e => e.EntryId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.FileDate)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("file_date");
            entity.Property(e => e.FileLink)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("file_link");
            entity.Property(e => e.FileSize)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("file_size");
            entity.Property(e => e.FiletypeId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("tinyint(4)")
                .HasColumnName("filetype_id");
            entity.Property(e => e.IsDemo)
                .HasColumnType("tinyint(1)")
                .HasColumnName("is_demo");
            entity.Property(e => e.MachinetypeId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("tinyint(4)")
                .HasColumnName("machinetype_id");
            entity.Property(e => e.Title)
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("title");
            entity.Property(e => e.Url)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("url");

            entity.HasOne(d => d.Entry).WithMany(p => p.Nvgs).HasForeignKey(d => d.EntryId);

            entity.HasOne(d => d.Filetype).WithMany(p => p.Nvgs).HasForeignKey(d => d.FiletypeId);

            entity.HasOne(d => d.Machinetype).WithMany(p => p.Nvgs).HasForeignKey(d => d.MachinetypeId);
        });

        modelBuilder.Entity<Origintype>(entity =>
        {
            entity.ToTable("origintypes");

            entity.HasIndex(e => e.Text, "IX_origintypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => new { e.WebsiteId, e.LabelId });

            entity.ToTable("permissions");

            entity.Property(e => e.WebsiteId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("website_id");
            entity.Property(e => e.LabelId)
                .HasColumnType("int(11)")
                .HasColumnName("label_id");
            entity.Property(e => e.PermissiontypeId)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("permissiontype_id");
            entity.Property(e => e.Text)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(300)")
                .HasColumnName("text");

            entity.HasOne(d => d.Label).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.LabelId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Permissiontype).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.PermissiontypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Website).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.WebsiteId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Permissiontype>(entity =>
        {
            entity.ToTable("permissiontypes");

            entity.HasIndex(e => e.Text, "IX_permissiontypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Platform>(entity =>
        {
            entity.ToTable("platforms");

            entity.HasIndex(e => e.Text, "IX_platforms_text").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("tinyint(4)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Port>(entity =>
        {
            entity.ToTable("ports");

            entity.HasIndex(e => new { e.EntryId, e.PlatformId, e.LinkSystem }, "IX_ports_entry_id_platform_id_link_system").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.IsOfficial)
                .HasColumnType("tinyint(1)")
                .HasColumnName("is_official");
            entity.Property(e => e.LinkSystem)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(200)")
                .HasColumnName("link_system");
            entity.Property(e => e.PlatformId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("platform_id");
            entity.Property(e => e.Title)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("title");

            entity.HasOne(d => d.Entry).WithMany(p => p.Ports)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Platform).WithMany(p => p.Ports)
                .HasForeignKey(d => d.PlatformId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Prefix>(entity =>
        {
            entity.HasKey(e => e.Text);

            entity.ToTable("prefixes");

            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(10)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Prefixexempt>(entity =>
        {
            entity.HasKey(e => e.Text);

            entity.ToTable("prefixexempts");

            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => new { e.EntryId, e.ReleaseSeq, e.LabelId });

            entity.ToTable("publishers");

            entity.HasIndex(e => new { e.EntryId, e.ReleaseSeq, e.PublisherSeq }, "IX_publishers_entry_id_release_seq_publisher_seq").IsUnique();

            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.ReleaseSeq)
                .HasColumnType("smallint(6)")
                .HasColumnName("release_seq");
            entity.Property(e => e.LabelId)
                .HasColumnType("int(11)")
                .HasColumnName("label_id");
            entity.Property(e => e.PublisherSeq)
                .HasDefaultValue((short)1)
                .HasColumnType("smallint(6)")
                .HasColumnName("publisher_seq");

            entity.HasOne(d => d.Entry).WithMany(p => p.Publishers)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Label).WithMany(p => p.Publishers)
                .HasForeignKey(d => d.LabelId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Release).WithMany(p => p.Publishers)
                .HasForeignKey(d => new { d.EntryId, d.ReleaseSeq })
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Referencetype>(entity =>
        {
            entity.ToTable("referencetypes");

            entity.HasIndex(e => e.Text, "IX_referencetypes_text").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("tinyint(4)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Relatedlicense>(entity =>
        {
            entity.HasKey(e => new { e.EntryId, e.LicenseId });

            entity.ToTable("relatedlicenses");

            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.LicenseId)
                .HasColumnType("int(11)")
                .HasColumnName("license_id");
            entity.Property(e => e.IsOfficial)
                .HasColumnType("tinyint(1)")
                .HasColumnName("is_official");

            entity.HasOne(d => d.Entry).WithMany(p => p.Relatedlicenses)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.License).WithMany(p => p.Relatedlicenses)
                .HasForeignKey(d => d.LicenseId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Relation>(entity =>
        {
            entity.HasKey(e => new { e.EntryId, e.OriginalId, e.RelationtypeId });

            entity.ToTable("relations");

            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.OriginalId)
                .HasColumnType("int(11)")
                .HasColumnName("original_id");
            entity.Property(e => e.RelationtypeId)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("relationtype_id");

            entity.HasOne(d => d.Entry).WithMany(p => p.RelationEntries)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Original).WithMany(p => p.RelationOriginals)
                .HasForeignKey(d => d.OriginalId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Relationtype).WithMany(p => p.Relations)
                .HasForeignKey(d => d.RelationtypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Relationtype>(entity =>
        {
            entity.ToTable("relationtypes");

            entity.HasIndex(e => e.Reciprocal, "IX_relationtypes_reciprocal").IsUnique();

            entity.HasIndex(e => e.Text, "IX_relationtypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("id");
            entity.Property(e => e.Comments)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("comments");
            entity.Property(e => e.Reciprocal)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("reciprocal");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Release>(entity =>
        {
            entity.HasKey(e => new { e.EntryId, e.ReleaseSeq });

            entity.ToTable("releases");

            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.ReleaseSeq)
                .HasColumnType("smallint(6)")
                .HasColumnName("release_seq");
            entity.Property(e => e.BookIsbn)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("book_isbn");
            entity.Property(e => e.BookPages)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("book_pages");
            entity.Property(e => e.BudgetPrice)
                .HasDefaultValueSql("NULL")
                .HasColumnType("decimal(9,2)")
                .HasColumnName("budget_price");
            entity.Property(e => e.CartridgePrice)
                .HasDefaultValueSql("NULL")
                .HasColumnType("decimal(9,2)")
                .HasColumnName("cartridge_price");
            entity.Property(e => e.CurrencyId)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("char(3)")
                .HasColumnName("currency_id");
            entity.Property(e => e.DiskPrice)
                .HasDefaultValueSql("NULL")
                .HasColumnType("decimal(9,2)")
                .HasColumnName("disk_price");
            entity.Property(e => e.MicrodrivePrice)
                .HasDefaultValueSql("NULL")
                .HasColumnType("decimal(9,2)")
                .HasColumnName("microdrive_price");
            entity.Property(e => e.ReleaseDay)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("release_day");
            entity.Property(e => e.ReleaseMonth)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("release_month");
            entity.Property(e => e.ReleasePrice)
                .HasDefaultValueSql("NULL")
                .HasColumnType("decimal(9,2)")
                .HasColumnName("release_price");
            entity.Property(e => e.ReleaseYear)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("release_year");

            entity.HasOne(d => d.Currency).WithMany(p => p.Releases).HasForeignKey(d => d.CurrencyId);

            entity.HasOne(d => d.Entry).WithMany(p => p.Releases)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Remake>(entity =>
        {
            entity.ToTable("remakes");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Authors)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("authors");
            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.FileDate)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("file_date");
            entity.Property(e => e.FileLink)
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("file_link");
            entity.Property(e => e.FileSize)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("file_size");
            entity.Property(e => e.Platforms)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(200)")
                .HasColumnName("platforms");
            entity.Property(e => e.RemakeStatus)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(1000)")
                .HasColumnName("remake_status");
            entity.Property(e => e.RemakeYears)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("remake_years");
            entity.Property(e => e.Title)
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("title");

            entity.HasOne(d => d.Entry).WithMany(p => p.Remakes)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => new { e.EntryId, e.LabelId, e.RoletypeId });

            entity.ToTable("roles");

            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.LabelId)
                .HasColumnType("int(11)")
                .HasColumnName("label_id");
            entity.Property(e => e.RoletypeId)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("roletype_id");

            entity.HasOne(d => d.Entry).WithMany(p => p.Roles)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Label).WithMany(p => p.Roles)
                .HasForeignKey(d => d.LabelId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Roletype).WithMany(p => p.Roles)
                .HasForeignKey(d => d.RoletypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Author).WithMany(p => p.Roles)
                .HasForeignKey(d => new { d.EntryId, d.LabelId })
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Roletype>(entity =>
        {
            entity.ToTable("roletypes");

            entity.HasIndex(e => e.Text, "IX_roletypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Schemetype>(entity =>
        {
            entity.ToTable("schemetypes");

            entity.HasIndex(e => e.Text, "IX_schemetypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(2)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasKey(e => new { e.WebsiteId, e.EntryId });

            entity.ToTable("scores");

            entity.Property(e => e.WebsiteId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("website_id");
            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.Score1)
                .HasColumnType("decimal(5,2)")
                .HasColumnName("score");
            entity.Property(e => e.Votes)
                .HasColumnType("int(11)")
                .HasColumnName("votes");

            entity.HasOne(d => d.Entry).WithMany(p => p.Scores)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Website).WithMany(p => p.Scores)
                .HasForeignKey(d => d.WebsiteId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Scrap>(entity =>
        {
            entity.ToTable("scraps");

            entity.HasIndex(e => e.FileLink, "IX_scraps_file_link").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.CasetypeId)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("casetype_id");
            entity.Property(e => e.Comments)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("comments");
            entity.Property(e => e.EntryId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.FileBarcode)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("file_barcode");
            entity.Property(e => e.FileCode)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("file_code");
            entity.Property(e => e.FileDate)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("file_date");
            entity.Property(e => e.FileDl)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(150)")
                .HasColumnName("file_dl");
            entity.Property(e => e.FileLink)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("file_link");
            entity.Property(e => e.FileSize)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("file_size");
            entity.Property(e => e.FiletypeId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("filetype_id");
            entity.Property(e => e.IsDemo)
                .HasColumnType("tinyint(1)")
                .HasColumnName("is_demo");
            entity.Property(e => e.LanguageId)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("char(2)")
                .HasColumnName("language_id");
            entity.Property(e => e.MachinetypeId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("tinyint(4)")
                .HasColumnName("machinetype_id");
            entity.Property(e => e.Rationale)
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("rationale");
            entity.Property(e => e.ReleaseSeq)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("release_seq");
            entity.Property(e => e.ReleaseYear)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("release_year");
            entity.Property(e => e.SchemetypeId)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("char(2)")
                .HasColumnName("schemetype_id");
            entity.Property(e => e.SourcetypeId)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("sourcetype_id");

            entity.HasOne(d => d.Casetype).WithMany(p => p.Scraps).HasForeignKey(d => d.CasetypeId);

            entity.HasOne(d => d.Entry).WithMany(p => p.Scraps).HasForeignKey(d => d.EntryId);

            entity.HasOne(d => d.Filetype).WithMany(p => p.Scraps)
                .HasForeignKey(d => d.FiletypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Language).WithMany(p => p.Scraps).HasForeignKey(d => d.LanguageId);

            entity.HasOne(d => d.Machinetype).WithMany(p => p.Scraps).HasForeignKey(d => d.MachinetypeId);

            entity.HasOne(d => d.Schemetype).WithMany(p => p.Scraps).HasForeignKey(d => d.SchemetypeId);

            entity.HasOne(d => d.Sourcetype).WithMany(p => p.Scraps).HasForeignKey(d => d.SourcetypeId);

            entity.HasOne(d => d.Release).WithMany(p => p.Scraps).HasForeignKey(d => new { d.EntryId, d.ReleaseSeq });
        });

        modelBuilder.Entity<SearchByAlias>(entity =>
        {
            entity.HasKey(e => new { e.EntryId, e.Title });

            entity.ToTable("search_by_aliases");

            entity.Property(e => e.EntryId).HasColumnName("entry_id");
            entity.Property(e => e.Title)
                .HasColumnType("varchar(250)")
                .HasColumnName("title");
            entity.Property(e => e.LibraryTitle)
                .UseCollation("nocase")
                .HasColumnType("varchar(300)")
                .HasColumnName("library_title");
        });

        modelBuilder.Entity<SearchByAuthor>(entity =>
        {
            entity.HasKey(e => new { e.LabelId, e.EntryId });

            entity.ToTable("search_by_authors");

            entity.HasIndex(e => e.EntryId, "ix_search_by_authors_entry");

            entity.Property(e => e.LabelId).HasColumnName("label_id");
            entity.Property(e => e.EntryId).HasColumnName("entry_id");
        });

        modelBuilder.Entity<SearchByIssue>(entity =>
        {
            entity.HasKey(e => e.IssueId);

            entity.ToTable("search_by_issues");

            entity.Property(e => e.IssueId)
                .ValueGeneratedNever()
                .HasColumnName("issue_id");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(300)")
                .HasColumnName("name");
        });

        modelBuilder.Entity<SearchByMagazine>(entity =>
        {
            entity.HasKey(e => new { e.MagazineId, e.LabelId });

            entity.ToTable("search_by_magazines");

            entity.HasIndex(e => e.LabelId, "ix_search_by_magazines_label");

            entity.Property(e => e.MagazineId).HasColumnName("magazine_id");
            entity.Property(e => e.LabelId).HasColumnName("label_id");
        });

        modelBuilder.Entity<SearchByMagref>(entity =>
        {
            entity.HasKey(e => new { e.EntryId, e.MagrefId });

            entity.ToTable("search_by_magrefs");

            entity.HasIndex(e => e.MagrefId, "ix_search_by_magrefs_magref");

            entity.Property(e => e.EntryId).HasColumnName("entry_id");
            entity.Property(e => e.MagrefId).HasColumnName("magref_id");
        });

        modelBuilder.Entity<SearchByName>(entity =>
        {
            entity.HasKey(e => new { e.LabelName, e.LabelId });

            entity.ToTable("search_by_names");

            entity.HasIndex(e => e.LabelId, "ix_search_by_names_label");

            entity.Property(e => e.LabelName)
                .UseCollation("nocase")
                .HasColumnType("varchar(100)")
                .HasColumnName("label_name");
            entity.Property(e => e.LabelId).HasColumnName("label_id");
        });

        modelBuilder.Entity<SearchByOrigin>(entity =>
        {
            entity.HasKey(e => e.EntryId);

            entity.ToTable("search_by_origins");

            entity.HasIndex(e => new { e.ContainerId, e.OrigintypeId }, "ix_search_by_origins_container");

            entity.HasIndex(e => new { e.IssueId, e.OrigintypeId }, "ix_search_by_origins_issue");

            entity.HasIndex(e => e.OrigintypeId, "ix_search_by_origins_type");

            entity.Property(e => e.EntryId)
                .ValueGeneratedNever()
                .HasColumnName("entry_id");
            entity.Property(e => e.ContainerId).HasColumnName("container_id");
            entity.Property(e => e.DateDay).HasColumnName("date_day");
            entity.Property(e => e.DateMonth).HasColumnName("date_month");
            entity.Property(e => e.DateYear).HasColumnName("date_year");
            entity.Property(e => e.IssueId).HasColumnName("issue_id");
            entity.Property(e => e.LibraryTitle)
                .UseCollation("nocase")
                .HasColumnType("varchar(300)")
                .HasColumnName("library_title");
            entity.Property(e => e.OrigintypeId)
                .HasColumnType("char(1)")
                .HasColumnName("origintype_id");
            entity.Property(e => e.Publication)
                .HasColumnType("varchar(300)")
                .HasColumnName("publication");
        });

        modelBuilder.Entity<SearchByPublisher>(entity =>
        {
            entity.HasKey(e => new { e.LabelId, e.EntryId });

            entity.ToTable("search_by_publishers");

            entity.HasIndex(e => e.EntryId, "ix_search_by_publishers_entry");

            entity.Property(e => e.LabelId).HasColumnName("label_id");
            entity.Property(e => e.EntryId).HasColumnName("entry_id");
        });

        modelBuilder.Entity<SearchByTitle>(entity =>
        {
            entity.HasKey(e => new { e.EntryTitle, e.EntryId });

            entity.ToTable("search_by_titles");

            entity.HasIndex(e => e.EntryId, "ix_search_by_titles_entry");

            entity.Property(e => e.EntryTitle)
                .UseCollation("nocase")
                .HasColumnType("varchar(250)")
                .HasColumnName("entry_title");
            entity.Property(e => e.EntryId).HasColumnName("entry_id");
        });

        modelBuilder.Entity<Sourcetype>(entity =>
        {
            entity.ToTable("sourcetypes");

            entity.HasIndex(e => e.Text, "IX_sourcetypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<SpexAuthor>(entity =>
        {
            entity.ToTable("spex_authors");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.LabelId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("label_id");
            entity.Property(e => e.Name)
                .UseCollation("rtrim")
                .HasColumnType("varchar(150)")
                .HasColumnName("name");

            entity.HasOne(d => d.Entry).WithMany(p => p.SpexAuthors)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Label).WithMany(p => p.SpexAuthors).HasForeignKey(d => d.LabelId);
        });

        modelBuilder.Entity<SpexEntry>(entity =>
        {
            entity.ToTable("spex_entries");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Diskprice)
                .HasColumnType("decimal(5,2)")
                .HasColumnName("diskprice");
            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.Fgtkey)
                .UseCollation("rtrim")
                .HasColumnType("varchar(150)")
                .HasColumnName("fgtkey");
            entity.Property(e => e.GenretypeId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("tinyint(4)")
                .HasColumnName("genretype_id");
            entity.Property(e => e.Orgprice)
                .HasColumnType("decimal(5,2)")
                .HasColumnName("orgprice");
            entity.Property(e => e.Pub1LabelId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("pub1_label_id");
            entity.Property(e => e.Pub2LabelId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("pub2_label_id");
            entity.Property(e => e.Pub3LabelId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("pub3_label_id");
            entity.Property(e => e.ReleaseSeq)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("release_seq");
            entity.Property(e => e.Repub2price)
                .HasColumnType("decimal(5,2)")
                .HasColumnName("repub2price");
            entity.Property(e => e.Repub3price)
                .HasColumnType("decimal(5,2)")
                .HasColumnName("repub3price");
            entity.Property(e => e.Title)
                .UseCollation("rtrim")
                .HasColumnType("varchar(150)")
                .HasColumnName("title");

            entity.HasOne(d => d.Entry).WithMany(p => p.SpexEntries)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Genretype).WithMany(p => p.SpexEntries).HasForeignKey(d => d.GenretypeId);

            entity.HasOne(d => d.Pub1Label).WithMany(p => p.SpexEntryPub1Labels).HasForeignKey(d => d.Pub1LabelId);

            entity.HasOne(d => d.Pub2Label).WithMany(p => p.SpexEntryPub2Labels).HasForeignKey(d => d.Pub2LabelId);

            entity.HasOne(d => d.Pub3Label).WithMany(p => p.SpexEntryPub3Labels).HasForeignKey(d => d.Pub3LabelId);

            entity.HasOne(d => d.Release).WithMany(p => p.SpexEntries).HasForeignKey(d => new { d.EntryId, d.ReleaseSeq });
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");

            entity.HasIndex(e => e.Name, "IX_tags_name").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Comments)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(1500)")
                .HasColumnName("comments");
            entity.Property(e => e.Link)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(200)")
                .HasColumnName("link");
            entity.Property(e => e.Name)
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("name");
            entity.Property(e => e.TagtypeId)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("tagtype_id");
            entity.Property(e => e.ToolId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("tool_id");

            entity.HasOne(d => d.Tagtype).WithMany(p => p.Tags)
                .HasForeignKey(d => d.TagtypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Tool).WithMany(p => p.Tags).HasForeignKey(d => d.ToolId);
        });

        modelBuilder.Entity<Tagtype>(entity =>
        {
            entity.ToTable("tagtypes");

            entity.HasIndex(e => e.Text, "IX_tagtypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Tool>(entity =>
        {
            entity.ToTable("tools");

            entity.HasIndex(e => new { e.PlatformId, e.Title }, "IX_tools_platform_id_title").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Authors)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("authors");
            entity.Property(e => e.Comments)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(500)")
                .HasColumnName("comments");
            entity.Property(e => e.LatestDate)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("latest_date");
            entity.Property(e => e.Link)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(200)")
                .HasColumnName("link");
            entity.Property(e => e.PlatformId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("platform_id");
            entity.Property(e => e.Title)
                .UseCollation("rtrim")
                .HasColumnType("varchar(250)")
                .HasColumnName("title");
            entity.Property(e => e.TooltypeId)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("tooltype_id");

            entity.HasOne(d => d.Platform).WithMany(p => p.Tools)
                .HasForeignKey(d => d.PlatformId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Tooltype).WithMany(p => p.Tools)
                .HasForeignKey(d => d.TooltypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Tooltype>(entity =>
        {
            entity.ToTable("tooltypes");

            entity.HasIndex(e => e.Text, "IX_tooltypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.ToTable("topics");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Comments)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(150)")
                .HasColumnName("comments");
            entity.Property(e => e.LabelId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("int(11)")
                .HasColumnName("label_id");
            entity.Property(e => e.MagazineId)
                .HasDefaultValueSql("NULL")
                .HasColumnType("smallint(6)")
                .HasColumnName("magazine_id");
            entity.Property(e => e.Name)
                .UseCollation("rtrim")
                .HasColumnType("varchar(150)")
                .HasColumnName("name");
            entity.Property(e => e.TopictypeId)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("topictype_id");

            entity.HasOne(d => d.Label).WithMany(p => p.Topics).HasForeignKey(d => d.LabelId);

            entity.HasOne(d => d.Magazine).WithMany(p => p.Topics).HasForeignKey(d => d.MagazineId);

            entity.HasOne(d => d.Topictype).WithMany(p => p.Topics)
                .HasForeignKey(d => d.TopictypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Topictype>(entity =>
        {
            entity.ToTable("topictypes");

            entity.HasIndex(e => e.Text, "IX_topictypes_text").IsUnique();

            entity.Property(e => e.Id)
                .UseCollation("rtrim")
                .HasColumnType("char(1)")
                .HasColumnName("id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");
        });

        modelBuilder.Entity<Webref>(entity =>
        {
            entity.HasKey(e => new { e.EntryId, e.Link });

            entity.ToTable("webrefs");

            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.Link)
                .UseCollation("rtrim")
                .HasColumnType("varchar(200)")
                .HasColumnName("link");
            entity.Property(e => e.LanguageId)
                .UseCollation("rtrim")
                .HasColumnType("char(2)")
                .HasColumnName("language_id");
            entity.Property(e => e.WebsiteId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("website_id");

            entity.HasOne(d => d.Entry).WithMany(p => p.Webrefs)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Language).WithMany(p => p.Webrefs)
                .HasForeignKey(d => d.LanguageId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Website).WithMany(p => p.Webrefs)
                .HasForeignKey(d => d.WebsiteId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Website>(entity =>
        {
            entity.ToTable("websites");

            entity.HasIndex(e => e.Name, "IX_websites_name").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("tinyint(4)")
                .HasColumnName("id");
            entity.Property(e => e.Comments)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("comments");
            entity.Property(e => e.Link)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("link");
            entity.Property(e => e.LinkMask)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("link_mask");
            entity.Property(e => e.Name)
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("name");
        });

        modelBuilder.Entity<ZxsrAward>(entity =>
        {
            entity.ToTable("zxsr_awards");

            entity.HasIndex(e => e.Text, "IX_zxsr_awards_text").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("tinyint(4)")
                .HasColumnName("id");
            entity.Property(e => e.MagazineId)
                .HasColumnType("smallint(6)")
                .HasColumnName("magazine_id");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnType("varchar(50)")
                .HasColumnName("text");

            entity.HasOne(d => d.Magazine).WithMany(p => p.ZxsrAwards)
                .HasForeignKey(d => d.MagazineId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ZxsrCaption>(entity =>
        {
            entity.HasKey(e => new { e.MagrefId, e.CaptionSeq });

            entity.ToTable("zxsr_captions");

            entity.Property(e => e.MagrefId)
                .HasColumnType("int(11)")
                .HasColumnName("magref_id");
            entity.Property(e => e.CaptionSeq)
                .HasColumnType("smallint(6)")
                .HasColumnName("caption_seq");
            entity.Property(e => e.IsBanner)
                .HasColumnType("tinyint(1)")
                .HasColumnName("is_banner");
            entity.Property(e => e.Text)
                .UseCollation("rtrim")
                .HasColumnName("text");

            entity.HasOne(d => d.Magref).WithMany(p => p.ZxsrCaptions)
                .HasForeignKey(d => d.MagrefId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ZxsrReview>(entity =>
        {
            entity.ToTable("zxsr_reviews");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.IntroText)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("longtext")
                .HasColumnName("intro_text");
            entity.Property(e => e.ReviewRating)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(2000)")
                .HasColumnName("review_rating");
            entity.Property(e => e.ReviewText)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("longtext")
                .HasColumnName("review_text");
        });

        modelBuilder.Entity<ZxsrScore>(entity =>
        {
            entity.ToTable("zxsr_scores");

            entity.HasIndex(e => new { e.MagrefId, e.ScoreSeq }, "IX_zxsr_scores_magref_id_score_seq").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Category)
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("category");
            entity.Property(e => e.Comments)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnName("comments");
            entity.Property(e => e.IsOverall)
                .HasColumnType("tinyint(1)")
                .HasColumnName("is_overall");
            entity.Property(e => e.MagrefId)
                .HasColumnType("int(11)")
                .HasColumnName("magref_id");
            entity.Property(e => e.Score)
                .HasDefaultValueSql("NULL")
                .UseCollation("rtrim")
                .HasColumnType("varchar(100)")
                .HasColumnName("score");
            entity.Property(e => e.ScoreSeq)
                .HasDefaultValueSql("NULL")
                .HasColumnType("tinyint(4)")
                .HasColumnName("score_seq");

            entity.HasOne(d => d.Magref).WithMany(p => p.ZxsrScores)
                .HasForeignKey(d => d.MagrefId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
