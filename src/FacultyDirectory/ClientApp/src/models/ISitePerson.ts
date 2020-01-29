import { ICorePersonProperties } from "./ICorePersonProperties";
import { IPerson } from "./IPerson";

export interface ISitePerson extends ICorePersonProperties {
    id: number;
    personId: number;
    person: IPerson;
    siteId: number;
    bio: string;
    shouldSync: boolean;
    lastSync: Date;
    lastUpdate: Date;
}
