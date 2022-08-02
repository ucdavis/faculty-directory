import { ICorePersonProperties } from "./ICorePersonProperties";
import { IPerson } from "./IPerson";

export interface ISitePerson extends ICorePersonProperties {
  id: number;
  pageUid: string;
  personId: number;
  person: IPerson;
  siteId: number;
  emails: string;
  phones: string;
  websites: string;
  pronunicationUid: string;
  tags: string;
  bio: string;
  shouldSync: boolean;
  lastSync: string;
  lastUpdate: string;
}
