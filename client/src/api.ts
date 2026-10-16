import type { ChatMessage, ChatResponse, ProblemDetails, RagResponse } from './types'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5079'

// POST /api/chat
export async function sendChat(messages: ChatMessage[]): Promise<ChatResponse> {
  const res = await callApi(`${API_URL}/api/chat`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ messages }),
  })
  return (await res.json()) as ChatResponse
}

// POST /api/rag/ask - answers from the Acme documents only
export async function askDocs(question: string): Promise<RagResponse> {
  const res = await callApi(`${API_URL}/api/rag/ask`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ question }),
  })
  return (await res.json()) as RagResponse
}

// GET /api/crash - always fails on purpose, to test ErrorHandlingMiddleware
export async function testCrash(): Promise<void> {
  await callApi(`${API_URL}/api/crash`, { method: 'GET' })
}

// Sends the request and turns every kind of failure into an Error with a readable message
async function callApi(url: string, options: RequestInit): Promise<Response> {
  let res: Response
  try {
    res = await fetch(url, options)
  } catch {
    // fetch only throws when there is no response at all (API not running, or blocked by CORS)
    throw new Error(
      `Cannot reach the API at ${API_URL}. Is the .NET project running? ` +
      `If it is, press F12 and check the Console for a CORS error.`
    )
  }

  if (res.ok) {
    return res
  }

  // Error responses from our API are ProblemDetails JSON
  const problem = (await res.json().catch(() => null)) as ProblemDetails | null

  // 400: validation errors -> show the messages from ChatRequestValidator / RagRequestValidator
  if (res.status === 400 && problem?.errors) {
    const messages = Object.values(problem.errors).flat()
    throw new Error(messages.join(' '))
  }

  // 500: from ErrorHandlingMiddleware -> show the title and the traceId to find it in the logs
  if (problem?.traceId) {
    throw new Error(`${problem.title ?? 'Server error.'} (status ${res.status}, traceId: ${problem.traceId})`)
  }

  throw new Error(`Request failed with status ${res.status}.`)
}
