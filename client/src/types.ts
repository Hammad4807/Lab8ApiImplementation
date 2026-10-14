// These types mirror the C# DTOs in the .NET API (Dtos folder)

// C#: ChatMessage
export type Role = 'user' | 'assistant'

export interface ChatMessage {
  role: Role
  content: string
}

// C#: ChatUsage (snake_case because of [JsonPropertyName])
export interface ChatUsage {
  prompt_tokens: number
  completion_tokens: number
  total_tokens: number
}

// C#: ChatResponse
export interface ChatResponse {
  reply: string
  usage: ChatUsage
}

// The error JSON the API sends back:
// 400 from [ApiController] + FluentValidation has "errors"
// 500 from ErrorHandlingMiddleware has "title" and "traceId"
export interface ProblemDetails {
  title?: string
  status?: number
  detail?: string
  traceId?: string
  errors?: Record<string, string[]>
}

// A message shown on screen. Assistant messages also remember
// how many messages were sent to get that reply (history on/off)
export interface DisplayMessage extends ChatMessage {
  sentCount?: number
}
