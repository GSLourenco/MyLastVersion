using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication2.GCM
{
   public static class Constants {

  /**
   * Endpoint for sending messages.
   */

       public static readonly String Project_key = "AIzaSyAkiO2WClVswdISSAmv2Sn0qEXPDfmr86U";

  public static readonly String GCM_SEND_ENDPOINT =
      "https://android.googleapis.com/gcm/send";

public static readonly String PARAM_PLAINTEXT_PAYLOAD_PREFIX = "data.";
  /**
   * HTTP parameter for registration id.
   */
  public static readonly String PARAM_REGISTRATION_ID = "registration_id";

  /**
   * HTTP parameter for collapse key.
   */
  public static readonly String PARAM_COLLAPSE_KEY = "collapse_key";

  /**
   * HTTP parameter for delaying the message delivery if the device is idle.
   */
  public static readonly String PARAM_DELAY_WHILE_IDLE = "delay_while_idle";

  /**
   * HTTP parameter for telling gcm to validate the message without actually sending it.
   */
  public static readonly String PARAM_DRY_RUN = "dry_run";

  /**
   * HTTP parameter for package name that can be used to restrict message delivery by matching
   * against the package name used to generate the registration id.
   */
  public static readonly String PARAM_RESTRICTED_PACKAGE_NAME = "restricted_package_name";

    /* Prefix to HTTP parameter used to set the message time-to-live.
   */
  public static readonly String PARAM_TIME_TO_LIVE = "time_to_live";

  /**
   * Missing registration_id.
   * Sender should always add the registration_id to the request.
   */
  public static readonly String ERROR_MISSING_REGISTRATION = "MissingRegistration";

  /**
   * Bad registration_id. Sender should remove this registration_id.
   */
  public static readonly String ERROR_INVALID_REGISTRATION = "InvalidRegistration";

  /**
   * The sender_id contained in the registration_id does not match the
   * sender_id used to register with the GCM servers.
   */
  public static readonly String ERROR_MISMATCH_SENDER_ID = "MismatchSenderId";

  /**
   * The user has uninstalled the application or turned off notifications.
   * Sender should stop sending messages to this device and delete the
   * registration_id. The client needs to re-register with the GCM servers to
   * receive notifications again.
   */
  public static readonly String ERROR_NOT_REGISTERED = "NotRegistered";

  public static readonly String ERROR_MESSAGE_RATE_EXCEEDED = "DeviceMessageRateExceeded";

  /**
   * Collapse key is required. Include collapse key in the request.
   */
  public static readonly String ERROR_MISSING_COLLAPSE_KEY = "MissingCollapseKey";

  /**
   * Time to Live value passed is less than zero or more than maximum.
   */
  public static readonly String ERROR_INVALID_TTL= "InvalidTtl";

  /**
   * Token returned by GCM when a message was successfully sent.
   */
  public static readonly String TOKEN_MESSAGE_ID = "id";

  /**
   * Token returned by GCM when the requested registration id has a canonical
   * value.
   */
  public static readonly String TOKEN_CANONICAL_REG_ID = "registration_id";

  /**
   * Token returned by GCM when there was an error sending a message.
   */
  public static readonly String TOKEN_ERROR = "Error";


 

  



    }
}