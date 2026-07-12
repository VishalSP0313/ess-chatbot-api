# ESS Chatbot API — .NET RAG Backend

ASP.NET Core Web API backend for the ESS Portal HR Policy chatbot, implementing RAG (Retrieval Augmented Generation) architecture using PostgreSQL with pgvector and Claude AI.

## 🛠️ Tech Stack
| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core (.NET 10) |
| Database | PostgreSQL 17 + pgvector |
| AI | Anthropic Claude API |
| ORM | Entity Framework Core + Npgsql |
| API Docs | Swagger / OpenAPI |

## ✨ Features

- **Semantic Search** — pgvector cosine similarity search
- **RAG Architecture** — retrieves relevant policy chunks before AI generation
- **Document Upload** — upload HR policy text and auto-chunk it
- **Claude AI Integration** — generates grounded answers from policy context
- **CORS configured** — works with Angular frontend on localhost:4200

## 📁 Project Structure
ESSChatbot/
├── Controllers/
│   └── ChatbotController.cs  → /ask, /upload, /health endpoints
├── Data/
│   └── AppDbContext.cs        → EF Core database context
├── Models/
│   └── HrPolicyChunk.cs      → Database model with vector column
├── Services/
│   ├── AnthropicService.cs   → Claude AI API integration
│   └── DocumentService.cs    → Text chunking + vector search
├── appsettings.json           → Configuration (placeholders)
└── Program.cs                 → App startup + DI registration
## 🗄️ Database Setup

```sql
-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create table
CREATE TABLE hr_policy_chunks (
    id BIGSERIAL PRIMARY KEY,
    content TEXT NOT NULL,
    source VARCHAR(255),
    chunk_index INTEGER,
    embedding vector(1536)
);
```

## 🏃 Running Locally

**Prerequisites:** .NET 10 SDK, PostgreSQL 17, pgvector

```bash
# Clone the repo
git clone https://github.com/VishalSP0313/ess-chatbot-api.git
cd ess-chatbot-api

# Create appsettings.Development.json with real credentials
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ess_chatbot;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Anthropic": {
    "ApiKey": "YOUR_ANTHROPIC_API_KEY"
  }
}

# Run the API
dotnet run
```

## 🔌 API Endpoints

| Method | Endpoint | Description |
|--------|---------|-------------|
| GET | /api/chatbot/health | Health check |
| POST | /api/chatbot/ask | Ask a question |
| POST | /api/chatbot/upload | Upload HR policy text |

## 📝 Example Usage

**Ask a question:**
```json
POST /api/chatbot/ask
{
  "question": "How many sick leaves do I get?"
}

Response:
{
  "answer": "According to HR policy, you are entitled to 12 sick leaves per year...",
  "chunksFound": 1
}
```

**Upload policy:**
```json
POST /api/chatbot/upload
{
  "content": "Sick Leave: Employees get 12 days per year...",
  "source": "HR Policy 2024"
}
```

## 🔮 RAG Architecture Flow
User Question
↓
Generate Embedding (1536 dimensions)
↓
pgvector Cosine Similarity Search
↓
Top 3 Relevant Policy Chunks
↓
Send Context + Question to Claude AI
↓
Grounded Answer Returned
## 🔗 Related Project

Angular Frontend: [ESS Portal](https://github.com/VishalSP0313/ess-portal)

## 👨‍💻 Author

**Vishal Prakash**
Full Stack Developer — .NET | Angular | PostgreSQL | AI