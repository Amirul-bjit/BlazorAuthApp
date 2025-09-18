# BlazorAuthApp

A modern, full-featured blog application built with Blazor Server, ASP.NET Core Identity, and Entity Framework Core. This application provides a complete blogging platform with user authentication, category management, and interactive features.

## 🚀 Features

### Authentication & Authorization
- User registration and login with ASP.NET Core Identity
- Email confirmation support
- Role-based access control
- External authentication providers support

### Blog Management
- Create, read, update, and delete blog posts
- Rich text editing capabilities
- Blog post publishing/unpublishing
- Author-specific blog management
- Soft delete functionality

### Category System
- Create and manage blog categories
- Category-based blog filtering
- Hierarchical category organization
- Category assignment to blog posts

### Interactive Features
- **Like System**: Users can like/unlike blog posts
- **Comment System**: Threaded commenting on blog posts
- Real-time like and comment counts
- User-specific interaction tracking

### Advanced Features
- **Pagination**: Efficient pagination for all list views
- **Search & Filtering**: Filter blogs by categories and sort options
- **Responsive Design**: Mobile-first responsive UI
- **Soft Delete**: Safe deletion with recovery options

## 🏗️ Architecture

### Service Layer Architecture
The application follows a clean service-oriented architecture:

```
Services/
├── BlogService.cs          # Core blog operations
├── BlogLikeService.cs      # Like/unlike functionality
├── BlogCommentService.cs   # Comment management
├── CategoryService.cs      # Category operations
└── Interfaces/             # Service contracts
```

### Key Services

- **[`BlogService`](Services/BlogService.cs)**: Manages blog CRUD operations, filtering, and sorting
- **[`BlogLikeService`](Services/BlogLikeService.cs)**: Handles blog like/unlike functionality
- **[`BlogCommentService`](Services/BlogCommentService.cs)**: Manages blog comments and threading
- **[`CategoryService`](Services/CategoryService.cs)**: Category management with pagination

### Data Layer
- **Entity Framework Core** for data access
- **SQL Server** database support
- **Code-First** migrations
- **[`ApplicationDbContext`](Data/ApplicationDbContext.cs)** for database operations

## 📱 Pages & Components

### Public Pages
- **Home**: Landing page with featured blogs
- **Blog List**: Paginated blog listing with filtering
- **Blog Details**: Individual blog post view with comments and likes
- **Categories**: Category listing and management

### Authentication Pages
- **Login/Register**: User authentication
- **Account Management**: Profile and security settings
- **External Login**: Third-party authentication support

### Admin Features
- **Category Management**: Create, edit, delete categories
- **Blog Management**: Author dashboard for blog posts
- **User Management**: Administrative user controls

## 🛠️ Technology Stack

- **Frontend**: Blazor Server, Bootstrap 5, Bootstrap Icons
- **Backend**: ASP.NET Core 8.0
- **Authentication**: ASP.NET Core Identity
- **Database**: Entity Framework Core with SQL Server
- **UI Components**: Ant Design Blazor
- **Styling**: Custom CSS with Bootstrap integration

## 🚦 Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/BlazorAuthApp.git
   cd BlazorAuthApp
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database connection string**
   Update the connection string in [`appsettings.json`](appsettings.json):
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BlazorAuthApp;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

4. **Run migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the application**
   Navigate to `https://localhost:5001` in your browser

## 🧪 User Acceptance Testing (UAT)

### Test Scenarios

#### Authentication Tests
- [ ] User can register with valid email and password
- [ ] User can login with correct credentials
- [ ] User cannot access protected pages without authentication
- [ ] Password reset functionality works correctly
- [ ] Email confirmation process is functional

#### Blog Management Tests
- [ ] Authenticated users can create new blog posts
- [ ] Authors can edit their own blog posts
- [ ] Authors can delete their own blog posts
- [ ] Blog posts display correctly with proper formatting
- [ ] Published/unpublished status works correctly

#### Category Management Tests
- [ ] Admin users can create new categories
- [ ] Categories can be edited and updated
- [ ] Categories can be deleted (soft delete)
- [ ] Blog posts can be assigned to categories
- [ ] Category filtering works on blog list

#### Interactive Features Tests
- [ ] Users can like/unlike blog posts
- [ ] Like counts update in real-time
- [ ] Users can add comments to blog posts
- [ ] Comment authors can edit/delete their comments
- [ ] Comment counts display correctly

#### Pagination & Search Tests
- [ ] Pagination works correctly on all list views
- [ ] Page size changes reflect immediately
- [ ] Category filtering shows correct results
- [ ] Blog sorting options work as expected

#### Responsive Design Tests
- [ ] Application works on mobile devices
- [ ] Navigation is accessible on small screens
- [ ] Forms are usable on touch devices
- [ ] All features work across different browsers

## 🤝 Contributing

We welcome contributions from the community! Here's how you can help:

### How to Contribute

1. **Fork the repository**
2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Make your changes**
4. **Add tests** for new functionality
5. **Commit your changes**
   ```bash
   git commit -m "Add: your feature description"
   ```
6. **Push to your branch**
   ```bash
   git push origin feature/your-feature-name
   ```
7. **Create a Pull Request**

### Areas for Contribution

- 🐛 **Bug Fixes**: Help us identify and fix bugs
- ✨ **New Features**: Implement new functionality
- 📚 **Documentation**: Improve documentation and examples
- 🎨 **UI/UX**: Enhance the user interface and experience
- 🧪 **Testing**: Add unit tests and integration tests
- 🔧 **Performance**: Optimize application performance
- 🌐 **Accessibility**: Improve accessibility features

### Development Guidelines

- Follow C# coding conventions
- Write meaningful commit messages
- Add XML documentation for public methods
- Ensure all tests pass before submitting
- Update relevant documentation

### Code Style
- Use meaningful variable and method names
- Follow async/await patterns consistently
- Implement proper error handling
- Add logging where appropriate

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Blazor community for inspiration and support
- Bootstrap team for the responsive CSS framework
- Ant Design Blazor for UI components

## 📧 Contact

For questions, suggestions, or support, please:
- Open an issue on GitHub
- Contact the maintainers
- Join our community discussions

---

**Built with ❤️ using Blazor Server and ASP.NET Core**