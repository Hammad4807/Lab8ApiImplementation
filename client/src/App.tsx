import { useEffect, useRef, useState } from 'react'
import { askDocs, sendChat, testCrash } from './api'
import type { ChatMessage, DisplayMessage, Mode } from './types'
import './App.css'

// Same limit as ChatRequestValidator on the server
const MAX_MESSAGES = 40

// Questions from the RAG workshop exercise (slide 20)
const DOCS_SUGGESTIONS = [
  'How long do I have to ask for a refund?',
  'How much is express shipping?',
  'Do you sell gift cards?',
  'How can I get my money back?',
]

function App() {
  const [mode, setMode] = useState<Mode>('chat')
  const [messages, setMessages] = useState<DisplayMessage[]>([])
  const [input, setInput] = useState('')
  const [sendHistory, setSendHistory] = useState(true)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [totalTokens, setTotalTokens] = useState<number | null>(null)

  const bottomRef = useRef<HTMLDivElement>(null)

  // Scroll to the newest message whenever the list changes
  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages, loading])

  async function handleSend(textToSend: string = input) {
    const text = textToSend.trim()
    if (!text || loading) return

    const newMessages: DisplayMessage[] = [...messages, { role: 'user', content: text }]

    // History ON: send the whole conversation. History OFF: send only the newest message.
    // The model has no memory - it only knows what we send in this request.
    // (Docs mode always sends only the question, so this limit only matters in chat mode.)
    const toSend: ChatMessage[] = (sendHistory ? newMessages : newMessages.slice(-1))
      .map(m => ({ role: m.role, content: m.content }))

    if (mode === 'chat' && toSend.length > MAX_MESSAGES) {
      setError('This chat reached the 40 message limit. Start a new chat or turn off history.')
      return
    }

    setMessages(newMessages)
    setInput('')
    setError('')
    setLoading(true)

    try {
      if (mode === 'chat') {
        const data = await sendChat(toSend)
        setMessages([...newMessages, { role: 'assistant', content: data.reply, sentCount: toSend.length }])
        setTotalTokens(data.usage.total_tokens)
      } else {
        // RAG: the server searches the Acme docs, then answers only from what it found
        const data = await askDocs(text)
        setMessages([...newMessages, { role: 'assistant', content: data.answer, sources: data.sources }])
        setTotalTokens(data.usage.total_tokens)
      }
    } catch (err) {
      // Put things back so the user can try again
      setMessages(messages)
      setInput(text)
      setError(err instanceof Error ? err.message : 'Something went wrong.')
    } finally {
      setLoading(false)
    }
  }

  async function handleCrashTest() {
    setError('')
    try {
      await testCrash()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong.')
    }
  }

  function handleNewChat() {
    setMessages([])
    setInput('')
    setError('')
    setTotalTokens(null)
  }

  // Switching mode starts fresh, so chat answers and docs answers are never mixed
  function handleModeChange(newMode: Mode) {
    if (newMode === mode) return
    setMode(newMode)
    handleNewChat()
  }

  // Enter sends, Shift+Enter makes a new line
  function handleKeyDown(e: React.KeyboardEvent<HTMLTextAreaElement>) {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      handleSend()
    }
  }

  return (
    <div className="app">
      <header className="header">
        <div>
          <h1>Groq Chat</h1>
          <p className="subtitle">React + TypeScript → .NET Web API → Groq</p>
        </div>
        <div className="header-buttons">
          <button className="secondary" onClick={handleCrashTest} disabled={loading}>
            Test error handling
          </button>
          <button onClick={handleNewChat} disabled={loading}>
            + New chat
          </button>
        </div>
      </header>

      <div className="mode-switch" role="tablist" aria-label="Mode">
        <button
          role="tab"
          aria-selected={mode === 'chat'}
          className={mode === 'chat' ? 'active' : ''}
          disabled={loading}
          onClick={() => handleModeChange('chat')}
        >
          💬 Chat
        </button>
        <button
          role="tab"
          aria-selected={mode === 'docs'}
          className={mode === 'docs' ? 'active' : ''}
          disabled={loading}
          onClick={() => handleModeChange('docs')}
        >
          📄 Ask Acme docs
        </button>
      </div>

      <div className="history-bar">
        {mode === 'chat' ? (
          <label className="history-switch">
            <input
              type="checkbox"
              checked={sendHistory}
              disabled={loading}
              onChange={e => setSendHistory(e.target.checked)}
            />
            Send chat history: <strong>{sendHistory ? 'On' : 'Off'}</strong>
          </label>
        ) : (
          <span>Answers come only from refund-policy, shipping and support-hours.</span>
        )}
        <span className="tokens">
          {totalTokens !== null ? `Last call: ${totalTokens} tokens` : 'No calls yet'}
        </span>
      </div>
      <p className="history-note">
        {mode === 'chat'
          ? (sendHistory
              ? 'Earlier messages are sent with your new message, so the model has context. '
              : 'Only your latest message is sent. The model cannot see earlier messages. ') +
            'Try: say "My name is Sam", then ask "What is my name?" with history on and off.'
          : 'Each question is searched on its own (no chat history). 0 tokens means nothing matched, so Groq was not called.'}
      </p>

      <main className="conversation">
        {messages.length === 0 && mode === 'chat' && (
          <div className="welcome">
            <h2>What's on your mind?</h2>
            <p>Ask a question about C#, .NET or anything else.</p>
          </div>
        )}

        {messages.length === 0 && mode === 'docs' && (
          <div className="welcome">
            <h2>Ask about Acme</h2>
            <p>Refunds, shipping and support hours. Try one:</p>
            <div className="suggestions">
              {DOCS_SUGGESTIONS.map(q => (
                <button key={q} className="secondary" disabled={loading} onClick={() => handleSend(q)}>
                  {q}
                </button>
              ))}
            </div>
          </div>
        )}

        {messages.map((m, index) => (
          <div key={index} className={`message ${m.role}`}>
            <div className="message-label">
              {m.role === 'user' ? 'You' : 'Groq'}
              {m.sentCount !== undefined && (
                <span className="sent-count">
                  {m.sentCount === 1 ? 'latest message only' : `${m.sentCount} messages sent`}
                </span>
              )}
              {m.sources !== undefined && (
                <span className="sent-count">
                  {m.sources.length === 0 ? 'no matching documents' : `${m.sources.length} sources`}
                </span>
              )}
            </div>
            <div className="message-text">{m.content || '(empty reply)'}</div>

            {m.sources !== undefined && m.sources.length > 0 && (
              <details className="sources">
                <summary>Sources</summary>
                {m.sources.map(s => (
                  <div key={s.number} className="source">
                    <div className="source-title">
                      [{s.number}] {s.source} <span className="score">score {s.score}</span>
                    </div>
                    <div className="source-text">{s.text}</div>
                  </div>
                ))}
              </details>
            )}
          </div>
        ))}

        {loading && <div className="thinking">{mode === 'docs' ? 'Searching the docs…' : 'Thinking…'}</div>}
        <div ref={bottomRef} />
      </main>

      {error && (
        <div className="error" role="alert">
          {error}
        </div>
      )}

      <form
        className="composer"
        onSubmit={e => {
          e.preventDefault()
          handleSend()
        }}
      >
        <textarea
          value={input}
          placeholder={
            mode === 'docs'
              ? 'Ask about refunds, shipping or support… (Enter to send)'
              : 'Type a message… (Enter to send, Shift+Enter for a new line)'
          }
          rows={2}
          maxLength={mode === 'docs' ? 1000 : 4000}
          disabled={loading}
          onChange={e => setInput(e.target.value)}
          onKeyDown={handleKeyDown}
        />
        <button type="submit" disabled={loading || input.trim() === ''}>
          Send
        </button>
      </form>
    </div>
  )
}

export default App
