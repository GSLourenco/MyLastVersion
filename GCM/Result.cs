using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace MvcApplication2.GCM
{
    public 
 class Result
    {
        private readonly String messageId;
        private readonly String canonicalRegistrationId;
        private readonly String errorCode;

    public class Builder {

    // optional parameters
    public String _messageId;
    public String _canonicalRegistrationId;
    public String _errorCode;

    public Builder canonicalRegistrationId(String value) {
      _canonicalRegistrationId = value;
      return this;
    }

    public Builder messageId(String value) {
      _messageId = value;
      return this;
    }

    public Builder errorCode(String value) {
      _errorCode = value;
      return this;
    }

    public Result build() {
      return new Result(this);
    }
  }

     private Result(Builder builder) {
    canonicalRegistrationId = builder._canonicalRegistrationId;
    messageId = builder._messageId;
    errorCode = builder._errorCode;
  }

  /**
   * Gets the message id, if any.
   */
  public String getMessageId() {
    return messageId;
  }

  /**
   * Gets the canonical registration id, if any.
   */
  public String getCanonicalRegistrationId() {
    return canonicalRegistrationId;
  }

  /**
   * Gets the error code, if any.
   */
  public String getErrorCodeName() {
    return errorCode;
  }

  
  public override String ToString() {
    StringBuilder builder = new StringBuilder("[");
    if (messageId != null) { 
      builder.Append(" messageId=").Append(messageId);
    }
    if (canonicalRegistrationId != null) {
      builder.Append(" canonicalRegistrationId=")
          .Append(canonicalRegistrationId);
    }
    if (errorCode != null) { 
      builder.Append(" errorCode=").Append(errorCode);
    }
    return builder.Append(" ]").ToString();
  }

    }
}