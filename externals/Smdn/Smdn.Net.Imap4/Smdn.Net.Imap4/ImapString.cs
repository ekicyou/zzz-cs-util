// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2008-2010 smdn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

namespace Smdn.Net.Imap4 {
  // string types:
  //   ImapString
  //     => handles 'string'
  //     ImapPreformattedString
  //       => handles pre-encoded/pre-formatted octets
  //     ImapQuotedString
  //       => handles 'quoted'
  //       ImapMailboxNameString (internal)
  //         => handles 'mailbox name'
  //     IImapLiteralString
  //       => handles 'literal'
  //     ImapNilString
  //       => handles 'NIL'
  //     ImapStringList
  //       => list of ImapString
  //       ImapParenthesizedString
  //         => handles 'parenthesized list'
  //     ImapStringEnum
  //       => string enumeration type
  //     ImapCombinableDataItem
  //       => combinable data item type

  public class ImapString : IEquatable<ImapString>, IEquatable<string> {
    protected internal string Value {
      get { return val; }
    }

    protected ImapString()
    {
      this.val = null;
    }

    public ImapString(string val)
    {
      if (val == null)
        throw new ArgumentNullException("val");

      this.val = val;
    }

    public static implicit operator ImapString(string str)
    {
      return new ImapString(str);
    }

    public static explicit operator string(ImapString str)
    {
      if (str == null)
        return null;
      else
        return str.ToString();
    }

    public override bool Equals(object obj)
    {
      var imapString = obj as ImapString;

      if (imapString != null)
        return Equals(imapString);

      var str = obj as string;

      if (str == null)
        return false;
      else
        return Equals(str);
    }

    public virtual bool Equals(string other)
    {
      return string.Equals(val, other, StringComparison.Ordinal);
    }

    public virtual bool Equals(ImapString other)
    {
      if (Object.ReferenceEquals(this, other))
        return true;
      else if (null == (object)other)
        return false;
      else
        return string.Equals(val, other.val, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
      return val.GetHashCode();
    }

    public override string ToString()
    {
      return val;
    }

    private /*readonly*/ string val;
  }
}
