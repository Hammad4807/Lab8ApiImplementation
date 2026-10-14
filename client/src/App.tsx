import { useEffect, useRef, useState } from 'react'
import { sendChat, testCrash } from './api'
import type { ChatMessage, DisplayMessage } from './types'
import './App.css'

// Same limit as ChatRequestValidator on the server
const MAX_MESSAGES = 40

function App() {
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

  async function handleSend() {
    const text = input.trim()
    if (!text || loading) return

    const newMessages: DisplayMessage[] = [...messages, { role: 'user', content: text }]

    // History ON: send the whole conversation. History OFF: send only the newest message.
    // The model has no memory - it only knows what we send in this request.
    const toSend: ChatMessage[] = (sendHistory ? newMessages : newMessages.slice(-1))
      .map(m => ({ role: m.role, content: m.content }))

    if (toSend.length > MAX_MESSAGES) {
      setError('This chat reached the 40 message limit. Start a new chat or turn off history.')
      return
    }

    setMessages(newMessages)
    setInput('')
    setError('')
    setLoading(true)

    try {
      const data = await sendChat(toSend)
      setMessages([...newMessages, { role: 'assistant', content: data.reply, sentCount: toSend.length }])
      setTotalTokens(data.usage.total_tokens)
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

      <div className="history-bar">
        <label className="history-switch">
          <input
            type="checkbox"
            checked={sendHistory}
            disabled={loading}
            onChange={e => setSendHistory(e.target.checked)}
          />
          Send chat history: <strong>{sendHistory ? 'On' : 'Off'}</strong>
        </label>
        <span className="tokens">
          {totalTokens !== null ? `Last call: ${totalTokens} tokens` : 'No calls yet'}
        </span>
      </div>
      <p className="history-note">
        {sendHistory
          ? 'Earlier messages are sent with your new message, so the model has context.'
          : 'Only your latest message is sent. The model cannot see earlier messages.'}
        {' '}Try: say "My name is Sam", then ask "What is my name?" with history on and off.
      </p>

      <main className="conversation">
        {messages.length === 0 && (
          <div className="welcome">
            <h2>What's on your mind?</h2>
            <p>Ask a question about C#, .NET or anything else.</p>
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
            </div>
            <div className="message-text">{m.content || '(empty reply)'}</div>
          </div>
        ))}

        {loading && <div className="thinking">Thinking…</div>}
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
          placeholder="Type a message… (Enter to send, Shift+Enter for a new line)"
          rows={2}
          maxLength={4000}
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
