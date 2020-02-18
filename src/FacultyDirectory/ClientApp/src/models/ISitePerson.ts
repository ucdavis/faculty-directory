import { ICorePersonProperties } from "./ICorePersonProperties";
import { IPerson } from "./IPerson";

export interface ISitePerson extends ICorePersonProperties {
    id: number;
    pageUid: string;
    personId: number;
    person: IPerson;
    siteId: number;
    bio: string;
    shouldSync: boolean;
    lastSync: string;
    lastUpdate: string;
}
