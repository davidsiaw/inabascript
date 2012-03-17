
using System;
using System.IO;
using System.Collections;



namespace InabaScript {
	internal class Token {
		public int kind;    // token kind
		public int pos;     // token position in the source text (starting at 0)
		public int col;     // token column (starting at 0)
		public int line;    // token line (starting at 1)
		public string val;  // token value
		public Token next;  // ML 2005-03-11 Peek tokens are kept in linked list
	}

	internal class Buffer {
		public const char EOF = (char) 256;
		const int MAX_BUFFER_LENGTH = 64 * 1024; // 64KB
		static byte[] buf;    // input buffer
		static int bufStart;  // position of first byte in buffer relative to input stream
		static int bufLen;    // length of buffer
		static int fileLen;   // length of input stream
		static int pos;       // current position in buffer
		static Stream stream; // input stream (seekable)
		static bool isUserStream; // was the stream opened by the user?
		
		public static void Fill (Stream s, bool isUserStream) {
			stream = s; Buffer.isUserStream = isUserStream;
			fileLen = bufLen = (int) s.Length;
			if (stream.CanSeek && bufLen > MAX_BUFFER_LENGTH) bufLen = MAX_BUFFER_LENGTH;
			buf = new byte[bufLen];
			bufStart = Int32.MaxValue; // nothing in the buffer so far
			Pos = 0; // setup  buffer to position 0 (start)
			if (bufLen == fileLen) Close();
		}
		
		// called at the end of Parser.Parse()
		public static void Close() {
			if (!isUserStream && stream != null) {
				stream.Close();
				stream = null;
			}
		}
		
		public static int Read () {
			if (pos < bufLen) {
				return buf[pos++];
			} else if (Pos < fileLen) {
				Pos = Pos; // shift buffer start to Pos
				return buf[pos++];
			} else {
				return EOF;
			}
		}

		public static int Peek () {
			if (pos < bufLen) {
				return buf[pos];
			} else if (Pos < fileLen) {
				Pos = Pos; // shift buffer start to pos
				return buf[pos];
			} else {
				return EOF;
			}
		}
		
		public static string GetString (int beg, int end) {
			int len = end - beg;
			char[] buf = new char[len];
			int oldPos = Pos;
			Pos = beg;
			for (int i = 0; i < len; ++i) buf[i] = (char) Read();
			Pos = oldPos;
			return new String(buf);
		}

		public static int Pos {
			get { return pos + bufStart; }
			set {
				if (value < 0) value = 0;
				else if (value > fileLen) value = fileLen;
				if (value >= bufStart && value < bufStart + bufLen) { // already in buffer
					pos = value - bufStart;
				} else if (stream != null) { // must be swapped in
					stream.Seek(value, SeekOrigin.Begin);
					bufLen = stream.Read(buf, 0, buf.Length);
					bufStart = value; pos = 0;
				} else {
					pos = fileLen - bufStart; // make Pos return fileLen
				}
			}
		}
	}

	internal class Scanner {
		const char EOL = '\n';
		const int eofSym = 0; /* pdt */
		const int charSetSize = 256;
	const int maxT = 20;
	const int noSym = 20;
	static short[] start = {
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0, 10,  0,  0,  0,  0,  0, 16, 17,  0,  0, 15,  3,  0,  0,
	 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 13, 19,  0, 21,  0,  0,
	  0,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,
	  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  0,  0,  0,  0,  0,
	  0,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
	  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, 18, 22, 20,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  7,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  -1};


		static Token t;          // current token
		static char ch;          // current input character
		static int pos;          // column number of current character
		static int line;         // line number of current character
		static int lineStart;    // start position of current line
		static int oldEols;      // EOLs that appeared in a comment;
		static BitArray ignore;  // set of characters to be ignored by the scanner

		static Token tokens;     // list of tokens already peeked (first token is a dummy)
		static Token pt;         // current peek token
		
		static char[] tval = new char[128]; // text of current token
		static int tlen;         // length of current token
		
		public static void Init (string fileName) {
			try {
				Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				Buffer.Fill(stream, false);
				Init();
			} catch (IOException) {
				Console.WriteLine("--- Cannot open file {0}", fileName);
	#if !WindowsCE
				System.Environment.Exit(1);
	#endif
			}
		}
		
		public static void Init (Stream s) {
			Buffer.Fill(s, true);
			Init();
		}

		private static void Init() {
			pos = -1; line = 1; lineStart = 0;
			oldEols = 0;
			NextCh();
			ignore = new BitArray(charSetSize+1);
			ignore[' '] = true;  // blanks are always white space
			ignore[9] = true; ignore[10] = true; ignore[13] = true; 
			pt = tokens = new Token();  // first token is a dummy
		}
		
		static void NextCh() {
			if (oldEols > 0) { ch = EOL; oldEols--; } 
			else {
				ch = (char)Buffer.Read(); pos++;
				// replace isolated '\r' by '\n' in order to make
				// eol handling uniform across Windows, Unix and Mac
				if (ch == '\r' && Buffer.Peek() != '\n') ch = EOL;
				if (ch == EOL) { line++; lineStart = pos + 1; }
			}
	
		}

		static void AddCh() {
			if (tlen >= tval.Length) {
				char[] newBuf = new char[2 * tval.Length];
				Array.Copy(tval, 0, newBuf, 0, tval.Length);
				tval = newBuf;
			}
			tval[tlen++] = ch;
			NextCh();
		}

	
	static bool Comment0() {
		int level = 1, line0 = line, lineStart0 = lineStart;
		NextCh();
		if (ch == '/') {
			NextCh();
			for(;;) {
				if (ch == 10) {
					level--;
					if (level == 0) { oldEols = line - line0; NextCh(); return true; }
					NextCh();
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
		} else {
			if (ch==EOL) {line--; lineStart = lineStart0;}
			pos = pos - 2; Buffer.Pos = pos+1; NextCh();
		}
		return false;
	}

	static bool Comment1() {
		int level = 1, line0 = line, lineStart0 = lineStart;
		NextCh();
		if (ch == '*') {
			NextCh();
			for(;;) {
				if (ch == '*') {
					NextCh();
					if (ch == '/') {
						level--;
						if (level == 0) { oldEols = line - line0; NextCh(); return true; }
						NextCh();
					}
				} else if (ch == '/') {
					NextCh();
					if (ch == '*') {
						level++; NextCh();
					}
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
		} else {
			if (ch==EOL) {line--; lineStart = lineStart0;}
			pos = pos - 2; Buffer.Pos = pos+1; NextCh();
		}
		return false;
	}


		static void CheckLiteral() {
			switch (t.val) {
			case "function": t.kind = 11; break;
			case "return": t.kind = 13; break;
			case "var": t.kind = 16; break;
			case "type": t.kind = 18; break;
			default: break;
		}
		}

		static Token NextToken() {
			while (ignore[ch]) NextCh();
			if (ch == '/' && Comment0() ||ch == '/' && Comment1()) return NextToken();
			t = new Token();
			t.pos = pos; t.col = pos - lineStart + 1; t.line = line; 
			int state = start[ch];
			tlen = 0; AddCh();
			
			switch (state) {
				case -1: { t.kind = eofSym; break; } // NextCh already done
				case 0: { t.kind = noSym; break; }   // NextCh already done
				case 1:
				if ((ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z')) {AddCh(); goto case 1;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 2:
				if ((ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z')) {AddCh(); goto case 2;}
				else {t.kind = 2; break;}
			case 3:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 4;}
				else {t.kind = noSym; break;}
			case 4:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 4;}
				else {t.kind = 3; break;}
			case 5:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 6;}
				else {t.kind = noSym; break;}
			case 6:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 6;}
				else {t.kind = 4; break;}
			case 7:
				if (ch == 187) {AddCh(); goto case 8;}
				else {t.kind = noSym; break;}
			case 8:
				if (ch == 191) {AddCh(); goto case 9;}
				else {t.kind = noSym; break;}
			case 9:
				{t.kind = 5; break;}
			case 10:
				if ((ch == 9 || ch >= ' ' && ch <= '!' || ch >= '#' && ch <= '$' || ch == '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= '_' || ch >= 'a' && ch <= '~' || ch == 163 || ch == 169 || ch == 174)) {AddCh(); goto case 10;}
				else if (ch == '"') {AddCh(); goto case 12;}
				else if (ch == 92) {AddCh(); goto case 11;}
				else {t.kind = noSym; break;}
			case 11:
				if ((ch == 92 || ch == 'n' || ch == 'r')) {AddCh(); goto case 10;}
				else {t.kind = noSym; break;}
			case 12:
				{t.kind = 6; break;}
			case 13:
				{t.kind = 7; break;}
			case 14:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 14;}
				else if (ch == '.') {AddCh(); goto case 5;}
				else {t.kind = 3; break;}
			case 15:
				{t.kind = 8; break;}
			case 16:
				{t.kind = 9; break;}
			case 17:
				{t.kind = 10; break;}
			case 18:
				{t.kind = 12; break;}
			case 19:
				{t.kind = 14; break;}
			case 20:
				{t.kind = 15; break;}
			case 21:
				{t.kind = 17; break;}
			case 22:
				{t.kind = 19; break;}

			}
			t.val = new String(tval, 0, tlen);
			return t;
		}
		
		// get the next token (possibly a token already seen during peeking)
		public static Token Scan () {
			if (tokens.next == null) {
				return NextToken();
			} else {
				pt = tokens = tokens.next;
				return tokens;
			}
		}

		// peek for the next token, ignore pragmas
		public static Token Peek () {
			if (pt.next == null) {
				do {
					pt = pt.next = NextToken();
				} while (pt.kind > maxT); // skip pragmas
			} else {
				do {
					pt = pt.next; 
				} while (pt.kind > maxT);
			}
			return pt;
		}
		
		// make sure that peeking starts at current scan position
		public static void ResetPeek () { pt = tokens; }

	} // end Scanner
}

